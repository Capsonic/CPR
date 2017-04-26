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

        protected override void define()
        {
            Dictionary<short, Dictionary<short, int>> colForYearVolume = new Dictionary<short, Dictionary<short, int>>();
            int colStartCurrentPeriod = 7, colStartSecondPeriod = 16, colStartThirdPeriod = 20;

            string sqlFiscalPeriod = File.ReadAllText(HostingEnvironment.MapPath("~/Reports/GrossMarginFutureByFiscalPeriod/FiscalPeriod.sql"));
            string sql = File.ReadAllText(HostingEnvironment.MapPath("~/Reports/GrossMarginFutureByFiscalPeriod/GrossMarginFutureByFiscalPeriod.sql"));

            using (var ctx = new FourthShiftContext())
            {
                #region SOURCE
                var fiscalPeriods = ctx.Database.SqlQuery<CurrentFiscalPeriod>(sqlFiscalPeriod,
                    new SqlParameter("@StartDate", paramDateFrom)).ToList();

                if (fiscalPeriods == null || fiscalPeriods.Count == 0)
                {
                    throw new Exception("The specified params resulted on an empty report");
                }

                DateTime dCurrentPeriodEndDate = fiscalPeriods[0].PeriodEndDate;
                DateTime dSecondPeriodEndDate = fiscalPeriods[1].PeriodEndDate;
                DateTime dThirdPeriodEndDate = fiscalPeriods[2].PeriodEndDate;

                var dataset = ctx.Database.SqlQuery<Item>(sql,
                        new SqlParameter("@StartDate", paramDateFrom),
                        new SqlParameter("@currentPeriodEndDate", dCurrentPeriodEndDate),
                        new SqlParameter("@secondPeriodEndDate", dSecondPeriodEndDate),
                        new SqlParameter("@thirdPeriodEndDate", dThirdPeriodEndDate)).ToList();
                #endregion

                #region DATASETS

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
                    .ToList();//TODO verify ItemControllingNetUnitPrice

                #region Past Due

                var pastDue = dataset
                   .Where(i => i.Period == "PastDue")
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

                       SUM_PastDue_Quantity = group.Sum(k => k.CORemainRequiredQuantity),
                       SUM_PastDue_Amount = group.Sum(k => k.CORemainingRequiredAmount),
                   }).ToList();
                #endregion

                #region Current Period

                var currentFiscalPeriod = dataset
                   .Where(i => i.Period == "Current")
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

                       //SUM_PastDue_Quantity = group.Sum(k => k.CORemainRequiredQuantity),
                       //SUM_PastDue_Amount = group.Sum(k => k.CORemainingRequiredAmount),
                       //SUM_CurrentPeriod_Quantity = group.Where(j => j.FutureOrCurrent == "Current").Sum(k => k.CORemainRequiredQuantity),
                       //SUM_CurrentPeriod_Amount = group.Where(j => j.FutureOrCurrent == "Current").Sum(k => k.CORemainingRequiredAmount)
                   }).ToList();
                #endregion

                #region Second Period
                var secondFiscalPeriod = dataset
                   .Where(i => i.Period == "SecondPeriod")
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

                       //SUM_PastDue_Quantity = group.Sum(k => k.CORemainRequiredQuantity),
                       //SUM_PastDue_Amount = group.Sum(k => k.CORemainingRequiredAmount),
                   }).ToList();
                #endregion

                #region Third Period
                var thirdFiscalPeriod = dataset
                   .Where(i => i.Period == "ThirdPeriod")
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

                       //SUM_PastDue_Quantity = group.Sum(k => k.CORemainRequiredQuantity),
                       //SUM_PastDue_Amount = group.Sum(k => k.CORemainingRequiredAmount),
                   }).ToList();
                #endregion

                #endregion

                #region DOCUMENT
                InitWorkBook("GrossMarginFutureByFiscalPeriod");
                CreateWorkSheet("Report");
                #endregion

                #region HEADER
                InsertTitle("Gross Margin Future By Fiscal Period");
                NewLine();

                InsertString("From: " + paramDateFrom + " to: " + dThirdPeriodEndDate, fontBold: true);

                //Current Period Header
                CurrentCol(colStartCurrentPeriod + 2);
                InsertSubtitle("Current Fiscal Period: " + dCurrentPeriodEndDate.ToShortDateString());

                //Second Period Header
                CurrentCol(colStartSecondPeriod);
                InsertSubtitle("Second Fiscal Period: " + dSecondPeriodEndDate.ToShortDateString());

                //Third Period Header
                CurrentCol(colStartThirdPeriod);
                InsertSubtitle("Third Fiscal Period: " + dThirdPeriodEndDate.ToShortDateString());

                NewLine();

                InsertLabel("Item Key");
                InsertLabel("Item Ref1", AlignMode: TextAlign.CENTER);
                InsertLabel("Item Number", AlignMode: TextAlign.CENTER);
                InsertLabel("Item Desc", AlignMode: TextAlign.CENTER);
                InsertLabel("Curr Inv", AlignMode: TextAlign.CENTER);
                InsertLabel("Unit Price", AlignMode: TextAlign.CENTER);

                InsertLabel("Past Due Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Past Due Amount", AlignMode: TextAlign.CENTER); //added

                InsertLabel("Current Sales Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Current Sales Amount", AlignMode: TextAlign.CENTER);

                InsertLabel("Net Shipped Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Net Shpped Amount", AlignMode: TextAlign.CENTER);

                InsertLabel("Total Sales Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Total Sales Amount", AlignMode: TextAlign.CENTER);
                
                InsertLabel("Margin Amount", AlignMode: TextAlign.CENTER);

                //Second Period
                InsertLabel("CO Qty", AlignMode: TextAlign.CENTER);
                InsertLabel("CO Amount", AlignMode: TextAlign.CENTER);
                InsertLabel("Total Sales Amount", AlignMode: TextAlign.CENTER);
                InsertLabel("Margin", AlignMode: TextAlign.CENTER);

                //Third Period
                InsertLabel("CO Qty", AlignMode: TextAlign.CENTER);
                InsertLabel("CO Amount", AlignMode: TextAlign.CENTER);
                InsertLabel("Total Sales Amount", AlignMode: TextAlign.CENTER);
                InsertLabel("Margin", AlignMode: TextAlign.CENTER);

                NewLine();
                CurrentCol(7);

                //Past Due Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Past Due Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.CURRENCY);


                //Current Sales Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Current Sales Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.CURRENCY);


                //Net Shipped Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.NUMBER);
                //Net Shipped Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Total Sales Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.NUMBER);
                //Total Sales Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Margin Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Second Period
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                    ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Third Period
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                    ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[allGroupped.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);


                NewLine();
                freeze();
                #endregion

                #region DETAIL

                foreach (var item in allGroupped)
                {
                    InsertNumber((decimal)item.Key.ItemKey);
                    InsertString(item.Key.ItemReference1);
                    InsertString(item.Key.ItemNumber);
                    InsertString(item.Key.ItemDescription);
                    InsertNumber(item.Key.Inventory);
                    InsertCurrency(item.Key.ItemControllingNetUnitPrice, Decimals: Precision.FOUR);

                    CurrentCol(colStartCurrentPeriod);
                    //Past Due
                    var pastDueForItem = pastDue.Where(e => e.ItemKey == item.Key.ItemKey).FirstOrDefault();
                    double pastDueForItemQty = pastDueForItem != null ? pastDueForItem.SUM_PastDue_Quantity : 0;
                    double pastDueForItemAmount = pastDueForItem != null ? pastDueForItem.SUM_PastDue_Amount : 0;
                    InsertNumber(pastDueForItemQty);
                    InsertCurrency(pastDueForItemAmount);

                    //Current Period
                    var currentFiscalPeriodForItem = currentFiscalPeriod.Where(e => e.ItemKey == item.Key.ItemKey).FirstOrDefault();
                    double currentFiscalPeriodForItemQty = currentFiscalPeriodForItem != null ? currentFiscalPeriodForItem.SUM_TotalSalesQuantity : 0;
                    double currentFiscalPeriodForItemAmount = currentFiscalPeriodForItem != null ? currentFiscalPeriodForItem.SUM_TotalSalesAmount : 0;
                    InsertNumber(currentFiscalPeriodForItemQty);
                    InsertCurrency(currentFiscalPeriodForItemAmount);

                    //Current Shipmnets
                    double currentFiscalShipmentsQty = currentFiscalPeriodForItem != null ? currentFiscalPeriodForItem.SUM_NetShippedQuantity : 0;
                    double currentFiscalShipmentsAmount = currentFiscalPeriodForItem != null ? currentFiscalPeriodForItem.SUM_NetShippedAmount : 0;
                    InsertNumber(currentFiscalShipmentsQty);
                    InsertCurrency(currentFiscalShipmentsAmount);

                    //Current Total Sales
                    InsertNumber(pastDueForItemQty + currentFiscalPeriodForItemQty + currentFiscalShipmentsQty);
                    InsertCurrency(pastDueForItemAmount + currentFiscalPeriodForItemAmount + currentFiscalShipmentsAmount);

                    //Current Margin
                    InsertCurrency(currentFiscalPeriodForItem != null ? currentFiscalPeriodForItem.SUM_MarginAmount + (pastDueForItem != null ? pastDueForItem.SUM_MarginAmount : 0) : 0);

                    //Second Period
                    CurrentCol(colStartSecondPeriod);
                    var secondFiscalPeriodForItem = secondFiscalPeriod.Where(e => e.ItemKey == item.Key.ItemKey).FirstOrDefault();

                    InsertNumber(secondFiscalPeriodForItem != null ? secondFiscalPeriodForItem.SUM_TotalSalesQuantity : 0);
                    InsertCurrency(secondFiscalPeriodForItem != null ? secondFiscalPeriodForItem.SUM_TotalSalesAmount : 0);
                    InsertCurrency(secondFiscalPeriodForItem != null ? secondFiscalPeriodForItem.SUM_TotalSalesAmount : 0);
                    InsertCurrency(secondFiscalPeriodForItem != null ? secondFiscalPeriodForItem.SUM_MarginAmount : 0);

                    //Third Period
                    CurrentCol(colStartThirdPeriod);
                    var thirdFiscalPeriodForItem = thirdFiscalPeriod.Where(e => e.ItemKey == item.Key.ItemKey).FirstOrDefault();

                    InsertNumber(thirdFiscalPeriodForItem != null ? thirdFiscalPeriodForItem.SUM_TotalSalesQuantity : 0);
                    InsertCurrency(thirdFiscalPeriodForItem != null ? thirdFiscalPeriodForItem.SUM_TotalSalesAmount : 0);
                    InsertCurrency(thirdFiscalPeriodForItem != null ? thirdFiscalPeriodForItem.SUM_TotalSalesAmount : 0);
                    InsertCurrency(thirdFiscalPeriodForItem != null ? thirdFiscalPeriodForItem.SUM_MarginAmount : 0);

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
                ws.Column(14).Width = 15;               //N
                ws.Column(15).Width = 15;               //O
                ws.Column(16).Width = 15;               //P
                ws.Column(17).Width = 15;               //Q
                ws.Column(18).Width = 15;               //R
                ws.Column(19).Width = 15;               //S
                ws.Column(20).Width = 15;               //T
                ws.Column(21).Width = 15;               //U
                ws.Column(22).Width = 15;               //B
                ws.Column(23).Width = 15;               //W

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

            public DateTime RequiredDate { get; set; }

            public double TotalSalesQuantity { get; set; }

            public double TotalSalesAmount { get; set; }
            public double MarginAmount { get; set; }
            //public string FutureOrCurrent { get; set; }
            public double NetShippedQuantity { get; set; }
            public double NetShippedAmount { get; set; }

            public string Period { get; set; }

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

        private class CurrentFiscalPeriod
        {
            public DateTime PeriodEndDate { get; set; }
            public short FiscalPeriod { get; set; }
        }

    }
}
