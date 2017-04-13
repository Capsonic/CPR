using Reports;
using ReportsWEBAPI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace ReportsMain
{
    public class GrossMarginFutureByFiscalPeriod : Report
    {
        public GrossMarginFutureByFiscalPeriod(string Owner) : base(Owner)
        {
        }

        public DateTime paramDateFrom { get; set; }
        public DateTime paramDateTo { get; set; }

        protected override void define()
        {
            Dictionary<short, Dictionary<short, int>> colForYearVolume = new Dictionary<short, Dictionary<short, int>>();
            int colStart = 14;
            int minFuturePeriod, maxFuturePeriod, countFuturePeriods;

            string sql = File.ReadAllText(HostingEnvironment.MapPath("~/Reports/GrossMarginFutureByFiscalPeriod/GrossMarginFutureByFiscalPeriod.sql"));

            using (var ctx = new FourthShiftContext())
            {

                #region DATASETS
                //paramDateFrom = DateTime.Parse("2017/3/6 12:00:00 AM");
                //paramDateTo = DateTime.Parse("2017/5/27 12:00:00 AM");
                var dataset = ctx.Database.SqlQuery<Item>(sql,
                        new SqlParameter("@StartDate", paramDateFrom),
                        new SqlParameter("@EndDate", paramDateTo)).ToList();

                var currentPeriodInfo = dataset.FirstOrDefault();
                if (currentPeriodInfo == null)
                {
                    throw new Exception("Report with current params results on an empty report.");
                }

                var allGroupped = dataset
                    .GroupBy(i => new
                    {
                        i.ItemKey,
                        i.ItemReference1,
                        i.ItemNumber,
                        i.ItemDescription,
                        i.Inventory
                    })
                    .OrderBy(i => i.Key.ItemReference1)
                    .ThenBy(i => i.Key.ItemNumber)     
                    .Select(i => new
                    {
                        Key = new
                        {
                            ItemKey = i.Key.ItemKey,
                            ItemReference1 = i.Key.ItemReference1,
                            ItemNumber = i.Key.ItemNumber,
                            ItemDescription = i.Key.ItemDescription,
                            Inventory = i.Key.Inventory,
                            ItemControllingNetUnitPrice = i.OrderByDescending(j => j.RequiredDate).Select(j => j.ItemControllingNetUnitPrice).FirstOrDefault()
                        }
                    })                    
                    .ToList();

                #region Current Period

                var currentFiscalPeriod = dataset
                   .Where(i => i.RequiredDatePeriodEndDate <= i.CurrentPeriodEndDate)
                   .GroupBy(i => new
                   {
                       i.ItemKey
                   }).Select(group => new Item
                   {
                       ItemKey = group.Key.ItemKey,
                       SUM_TotalSalesQuantity = group.Sum(i => i.TotalSalesQuantity),
                       SUM_MarginAmount = group.Sum(i => i.MarginAmount),
                       SUM_NetShippedAmount = group.Sum(i => i.NetShippedAmount),
                       SUM_NetShippedQuantity = group.Sum(i => i.NetShippedQuantity),
                       SUM_TotalSalesAmount = group.Sum(i => i.TotalSalesAmount),

                       SUM_PastDue_Quantity = group.Where(j => j.FutureOrCurrent == "PastDue").Sum(k => k.CORemainRequiredQuantity),
                       SUM_PastDue_Amount = group.Where(j => j.FutureOrCurrent == "PastDue").Sum(k => k.CORemainingRequiredAmount),

                       //SUM_CurrentPeriod_Quantity = group.Where(j => j.FutureOrCurrent == "Current").Sum(k => k.CORemainRequiredQuantity),
                       //SUM_CurrentPeriod_Amount = group.Where(j => j.FutureOrCurrent == "Current").Sum(k => k.CORemainingRequiredAmount)
                   }).ToList();

                var join = from item in allGroupped
                           join curr in currentFiscalPeriod on item.Key.ItemKey equals curr.ItemKey into c
                           from currOrEmpty in c.DefaultIfEmpty()
                               //join future in futurePeriods on item.Key.ItemKey equals future.ItemKey into f
                               //from futureOrEmpty in f.DefaultIfEmpty()
                           select new
                           {
                               grouppedItem = item,
                               currentP = currOrEmpty
                               //futureP = futureOrEmpty
                           };
                #endregion

                #region Future Period
                var futurePeriods = dataset
                    .Where(i => i.RequiredDatePeriodEndDate > i.CurrentPeriodEndDate)
                    .GroupBy(i => new
                    {
                        i.ItemKey,
                        i.RequiredDateFiscalYear,
                        i.RequiredDateFiscalPeriod
                    }).Select(group => new Item
                    {
                        ItemKey = group.Key.ItemKey,
                        RequiredDateFiscalYear = group.Key.RequiredDateFiscalYear,
                        RequiredDateFiscalPeriod = group.Key.RequiredDateFiscalPeriod,                        
                        SUM_TotalSalesQuantity = group.Sum(i => i.TotalSalesQuantity),
                        SUM_TotalSalesAmount = group.Sum(i => i.TotalSalesAmount),
                        SUM_MarginAmount = group.Sum(i => i.MarginAmount)
                    }).ToList();

                var theFuturePeriods = dataset
                                       .Where(i => i.RequiredDatePeriodEndDate > i.CurrentPeriodEndDate)
                                       .GroupBy(i => new
                                       {
                                           i.RequiredDateFiscalYear,
                                           i.RequiredDateFiscalPeriod
                                       })
                                       .Select(i => i.Key)
                                       .OrderBy(i => i.RequiredDateFiscalYear)
                                       .ThenBy(i => i.RequiredDateFiscalPeriod)
                                       .ToList();
                
                minFuturePeriod = theFuturePeriods.First().RequiredDateFiscalPeriod;
                maxFuturePeriod = theFuturePeriods.Last().RequiredDateFiscalPeriod;
                countFuturePeriods = theFuturePeriods.Count();

                for (int i = 0; i < countFuturePeriods; i++)
                {
                    if (!colForYearVolume.ContainsKey(theFuturePeriods.ElementAt(i).RequiredDateFiscalYear))
                    {
                        colForYearVolume.Add(theFuturePeriods.ElementAt(i).RequiredDateFiscalYear, new Dictionary<short, int>());
                    }
                    colForYearVolume[theFuturePeriods.ElementAt(i).RequiredDateFiscalYear].Add(theFuturePeriods.ElementAt(i).RequiredDateFiscalPeriod, colStart);
                    colStart += 4;
                }
                #endregion

                #endregion

                #region DOCUMENT
                InitWorkBook("GrossMarginFutureByFiscalPeriod");
                CreateWorkSheet("Report");
                #endregion
                
                #region HEADER
                InsertTitle("Gross Margin Future By Fiscal Period");
                NewLine();

                InsertString("From: " + paramDateFrom + " to: " + paramDateTo, fontBold: true);

                //Current Period Header
                CurrentCol(7);
                InsertSubtitle(currentPeriodInfo.CurrentFiscalYear + " - Current Fiscal Period: " + currentPeriodInfo.CurrrentFiscalPeriod);

                //Future Period Headers
                foreach (var item in theFuturePeriods)
                {
                    CurrentCol(colForYearVolume[item.RequiredDateFiscalYear][item.RequiredDateFiscalPeriod]);
                    InsertSubtitle(item.RequiredDateFiscalYear + " - Period: " + item.RequiredDateFiscalPeriod);
                }
                NewLine();

                InsertLabel("Item Key");
                InsertLabel("Item Ref1", AlignMode: TextAlign.CENTER);
                InsertLabel("Item Number", AlignMode: TextAlign.CENTER);
                InsertLabel("Item Desc", AlignMode: TextAlign.CENTER);
                InsertLabel("Curr Inv", AlignMode: TextAlign.CENTER);
                InsertLabel("Unit Price", AlignMode: TextAlign.CENTER);

                InsertLabel("Total Sales Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Total Sales Amount", AlignMode: TextAlign.CENTER);

                InsertLabel("Current Sales Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Current Sales Amount", AlignMode: TextAlign.CENTER);

                InsertLabel("Past Due Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Past Due Amount", AlignMode: TextAlign.CENTER); //added

                InsertLabel("Net Shipped Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Net Shpped Amount", AlignMode: TextAlign.CENTER);

                InsertLabel("Margin Amount", AlignMode: TextAlign.CENTER);

                foreach (var item in theFuturePeriods)
                {
                    CurrentCol(colForYearVolume[item.RequiredDateFiscalYear][item.RequiredDateFiscalPeriod]);
                    ws.Column(CurrentCol()).Width = 15;
                    InsertLabel("CO Qty", AlignMode: TextAlign.CENTER);
                    ws.Column(CurrentCol()).Width = 15;
                    InsertLabel("CO Amount", AlignMode: TextAlign.CENTER);
                    ws.Column(CurrentCol()).Width = 15;
                    InsertLabel("Total Sales Amount", AlignMode: TextAlign.CENTER);
                    ws.Column(CurrentCol()).Width = 15;
                    InsertLabel("Margin", AlignMode: TextAlign.CENTER);
                    ws.Column(CurrentCol()).Width = 15;
                }

                NewLine();
                CurrentCol(6);
                InsertLabel("Totals:", AlignMode: TextAlign.RIGHT);
                //Total Sales Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Total Sales Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Current Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Current Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Type: NumberTypes.CURRENCY);

                //Past Due Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Past Due Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Type: NumberTypes.CURRENCY);

                //Net Shipped Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.NUMBER);
                //Net Shipped Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Margin Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                foreach (var item in theFuturePeriods)
                {
                    CurrentCol(colForYearVolume[item.RequiredDateFiscalYear][item.RequiredDateFiscalPeriod]);
                    InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                    InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                          ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                    InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                          ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                    InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                          ws.Cells[join.Count() + 3, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                }

                NewLine();
                freeze();
                #endregion

                #region DETAIL


                foreach (var item in join)
                {
                    InsertNumber((decimal)item.grouppedItem.Key.ItemKey);
                    InsertString(item.grouppedItem.Key.ItemReference1);
                    InsertString(item.grouppedItem.Key.ItemNumber);
                    InsertString(item.grouppedItem.Key.ItemDescription);
                    InsertNumber(item.grouppedItem.Key.Inventory);
                    InsertCurrency(item.grouppedItem.Key.ItemControllingNetUnitPrice, Decimals: Precision.FOUR);

                    if (item.currentP != null)
                    {
                        InsertNumber(item.currentP.SUM_TotalSalesQuantity);
                        InsertCurrency(item.currentP.SUM_TotalSalesAmount, Decimals: Precision.TWO);

                        InsertNumber(item.currentP.SUM_TotalSalesQuantity - item.currentP.SUM_PastDue_Quantity);
                        InsertCurrency(item.currentP.SUM_TotalSalesAmount - item.currentP.SUM_PastDue_Amount);

                        InsertNumber(item.currentP.SUM_PastDue_Quantity);
                        InsertCurrency(item.currentP.SUM_PastDue_Amount);

                        InsertNumber(item.currentP.SUM_NetShippedQuantity);                        
                        InsertCurrency(item.currentP.SUM_NetShippedAmount, Decimals: Precision.TWO);

                        InsertCurrency(item.currentP.SUM_MarginAmount, Decimals: Precision.TWO);
                    }
                    var futureList = futurePeriods.Where(f => f.ItemKey == item.grouppedItem.Key.ItemKey);
                    foreach (var fut in futureList)
                    {
                        CurrentCol(colForYearVolume[fut.RequiredDateFiscalYear][fut.RequiredDateFiscalPeriod]);
                        InsertNumber(fut.SUM_TotalSalesQuantity);
                        InsertCurrency(fut.SUM_TotalSalesAmount, Decimals: Precision.TWO);
                        InsertCurrency(fut.SUM_TotalSalesAmount, Decimals: Precision.TWO);
                        InsertCurrency(fut.SUM_MarginAmount, Decimals: Precision.TWO);
                    }

                    NewLine();
                }
                #endregion

                #region LAYOUT
                ws.Column(1).Width = 8;                 //A
                ws.Column(2).Width = 8;                 //B
                ws.Column(3).Width = 15;                //C
                ws.Column(4).Width = 30;                //D
                ws.Column(5).Width = 8;                 //E
                ws.Column(6).Width = 15;                //F
                ws.Column(7).Width = 15;                //G
                ws.Column(8).Width = 15;                //H
                ws.Column(9).Width = 15;                //I
                ws.Column(10).Width = 15;               //J
                ws.Column(11).Width = 15;               //K
                ws.Column(12).Width = 15;               //L
                ws.Column(13).Width = 15;               //M
                ws.Column(14).Width = 15;               //L
                ws.Column(15).Width = 15;               //M

                deduceColPageBreak();
                #endregion

            }
        }

        private class Item
        {
            public int ItemKey { get; set; }
            public string ItemReference1 { get; set; }
            public string ItemNumber { get; set; }
            public string ItemDescription { get; set; }
            public double Inventory { get; set; }
            public double ItemControllingNetUnitPrice { get; set; }

            public short CurrrentFiscalPeriod { get; set; }
            public short CurrentFiscalYear { get; set; }
            
            public DateTime RequiredDatePeriodEndDate { get; set; }
            public DateTime CurrentPeriodEndDate { get; set; }
            public DateTime RequiredDate { get; set; }

            public double TotalSalesQuantity { get; set; }
           
            public double TotalSalesAmount { get; set; }
            public double MarginAmount { get; set; }
            //public string FutureOrCurrent { get; set; }
            public double NetShippedQuantity { get; set; }
            public double NetShippedAmount { get; set; }

            public string FutureOrCurrent { get; set; }

            //public decimal? PDQuantity
            //{
            //    get
            //    {
            //        if (FutureOrCurrent == "PastDue")
            //        {
            //            return CORemainRequiredQuantity;
            //        }
            //        return 0;
            //    }
            //}

            public short RequiredDateFiscalYear { get; set; }
            public short RequiredDateFiscalPeriod { get; set; }


            public double SUM_TotalSalesQuantity { get; set; }
            public double SUM_NetShippedQuantity { get; set; }
            public double SUM_TotalSalesAmount { get; set; }
            public double SUM_NetShippedAmount { get; set; }
            public double SUM_MarginAmount { get; set; }


            #region For Current Period Only
            public double CORemainRequiredQuantity { get; set; }
            public double CORemainingRequiredAmount { get; set; }

            public double SUM_PastDue_Quantity { get; set; }
            public double SUM_PastDue_Amount { get; set; }
            //public double SUM_CurrentPeriod_Quantity { get; set; }
            //public double SUM_CurrentPeriod_Amount { get; set; }
            #endregion

        }

    }
}
