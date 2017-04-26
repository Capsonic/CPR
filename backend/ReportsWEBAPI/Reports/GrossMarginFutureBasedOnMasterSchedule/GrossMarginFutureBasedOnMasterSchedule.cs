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
    public class GrossMarginFutureBasedOnMasterSchedule : Report
    {
        public GrossMarginFutureBasedOnMasterSchedule(string Owner) : base(Owner)
        {
        }

        public DateTime paramDateFrom { get; set; }

        protected override void define()
        {
            Dictionary<short, Dictionary<short, int>> colForYearVolume = new Dictionary<short, Dictionary<short, int>>();
            int colStartCurrentPeriod = 7, colStartSecondPeriod = 14, colStartThirdPeriod = 18;

            string sqlFiscalPeriod = File.ReadAllText(HostingEnvironment.MapPath("~/Reports/GrossMarginFutureBasedOnMasterSchedule/FiscalPeriod.sql"));
            string sql = File.ReadAllText(HostingEnvironment.MapPath("~/Reports/GrossMarginFutureBasedOnMasterSchedule/GrossMarginFutureBasedOnMasterSchedule.sql"));

            using (var ctx = new FourthShiftContext())
            {

                #region DATASETS
                //paramDateFrom = DateTime.Parse("2017/3/6 12:00:00 AM");
                //paramDateTo = DateTime.Parse("2017/5/27 12:00:00 AM");

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

                #region DOCUMENT
                InitWorkBook("GrossMarginFutureByFiscalPeriod");
                CreateWorkSheet("Report");
                #endregion

                #region HEADER
                InsertTitle("Gross Margin Future By Fiscal Period");
                NewLine();

                InsertString("From: " + paramDateFrom, fontBold: true);

                //Current Period Header
                CurrentCol(colStartCurrentPeriod);
                InsertSubtitle("Current Fiscal Period: " + dCurrentPeriodEndDate.ToString("MMM yyy"));
                
                //Second Period Header
                CurrentCol(colStartSecondPeriod);
                InsertSubtitle(dSecondPeriodEndDate.ToString("MMM yyy"));
                
                //Third Period Header
                CurrentCol(colStartThirdPeriod);
                InsertSubtitle(dThirdPeriodEndDate.ToString("MMM yyy"));

                NewLine();

                InsertLabel("Item Key");
                InsertLabel("Item Ref1", AlignMode: TextAlign.CENTER);
                InsertLabel("Item Number", AlignMode: TextAlign.CENTER);
                InsertLabel("Item Desc", AlignMode: TextAlign.CENTER);
                InsertLabel("Curr Inv", AlignMode: TextAlign.CENTER);
                InsertLabel("Unit Price", AlignMode: TextAlign.CENTER);

                InsertLabel("Sales Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Sales Amount", AlignMode: TextAlign.CENTER);

                InsertLabel("Past Due Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Past Due Amount", AlignMode: TextAlign.CENTER); //added

                InsertLabel("Net Shipped Quantity", AlignMode: TextAlign.CENTER);
                InsertLabel("Net Shpped Amount", AlignMode: TextAlign.CENTER);

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
                CurrentCol(6);
                InsertLabel("Totals:", AlignMode: TextAlign.RIGHT);
                
                //Current Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Current Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.CURRENCY);

                //Past Due Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                //Past Due Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.CURRENCY);

                //Net Shipped Quantity
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.NUMBER);
                //Net Shipped Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Margin Amount
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                      ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Second Period
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                    ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);

                //Third Period
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                    ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Type: NumberTypes.NUMBER);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);
                InsertFormula("=SUM(" + ws.Cells[CurrentRow() + 1, CurrentCol()].Address + ":" +
                                        ws.Cells[dataset.Count() + 4, CurrentCol()].Address + ")", Decimals: Precision.TWO, Type: NumberTypes.CURRENCY);


                NewLine();
                freeze();
                #endregion

                #region DETAIL


                foreach (var item in dataset)
                {
                    InsertNumber((decimal)item.ItemKey);
                    InsertString(item.ItemRef1);
                    InsertString(item.ITEM);
                    InsertString(item.ItemDesc);
                    InsertNumber(item.Inventory);
                    InsertCurrency(item.CostType1, Decimals: Precision.FOUR);

                    //Current Period
                    InsertNumber(item.CurrentPeriodQty);
                    InsertCurrency(item.CurrentPeriodAmount, Decimals: Precision.TWO);

                    //Past Due
                    InsertNumber(item.PAST_DUE_QTY);
                    InsertCurrency(item.PAST_DUE_AMOUNT, Decimals: Precision.TWO);

                    InsertNumber(item.NetShippedQuantity ?? 0);
                    InsertCurrency(item.NetShippedAmount ?? 0, Decimals: Precision.TWO);

                    //Margin
                    InsertCurrency(item.MarginCO ?? 0 + item.MarginShipment ?? 0, Decimals: Precision.TWO);

                    
                    //Second Period
                    InsertNumber(item.SecondPeriodQty);
                    InsertCurrency(item.SecondPeriodAmount, Decimals: Precision.TWO);
                    InsertCurrency(item.SecondPeriodAmount, Decimals: Precision.TWO);
                    InsertCurrency(null, Decimals: Precision.TWO);

                    
                    
                    //Third Period
                    InsertNumber(item.ThirdPeriodQty);
                    InsertCurrency(item.ThirdPeriodAmount, Decimals: Precision.TWO);
                    InsertCurrency(item.ThirdPeriodAmount, Decimals: Precision.TWO);
                    InsertCurrency(null, Decimals: Precision.TWO);
                    
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
            public string ItemRef1 { get; set; }
            public string ITEM { get; set; }
            public string ItemDesc { get; set; }
            public double Inventory { get; set; }
            public double CostType1 { get; set; }
            public double PAST_DUE_QTY { get; set; }
            public double PAST_DUE_AMOUNT { get; set; }
            public double CurrentPeriodQty { get; set; }
            public double CurrentPeriodAmount { get; set; }
            public double SecondPeriodQty { get; set; }
            public double SecondPeriodAmount { get; set; }
            public double ThirdPeriodQty { get; set; }
            public double ThirdPeriodAmount { get; set; }
            public double TotalRolledCost { get; set; }
            public double? NetShippedQuantity { get; set; }
            public double? NetShippedAmount { get; set; }
            public double? MarginShipment { get; set; }
            public double? MarginCO { get; set; }
        }

        private class CurrentFiscalPeriod
        {
            public DateTime PeriodEndDate { get; set; }
            public short FiscalPeriod { get; set; }
        }

    }
}
