SELECT PastDueAndCurrentCOs.ItemNumber
	,PastDueAndCurrentCOs.ItemKey
	,PastDueAndCurrentCOs.ItemDescription
	,PastDueAndCurrentCOs.MakeBuyCode
	,PastDueAndCurrentCOs.ItemReference1
	,PastDueAndCurrentCOs.TotalRolledCost
	,PastDueAndCurrentCOs.CurrentFiscalYear
	,PastDueAndCurrentCOs.CurrrentFiscalPeriod
	,PastDueAndCurrentCOs.CurrentPeriodEndDate
	,PastDueAndCurrentCOs.RequiredDate
	,PastDueAndCurrentCOs.RequiredDateFiscalYear
	,PastDueAndCurrentCOs.RequiredDateFiscalPeriod
	,PastDueAndCurrentCOs.RequiredDatePeriodEndDate
	,CASE 
		WHEN PastDueAndCurrentCOs.CurrentPeriodEndDate < RequiredDatePeriodEndDate
			THEN 'Future'
		ELSE CASE 
				WHEN PastDueAndCurrentCOs.CurrentPeriodEndDate = RequiredDatePeriodEndDate
					THEN 'Current'
				ELSE 'PastDue'
				END
		END AS FutureOrCurrent
	,PastDueAndCurrentCOs.ViewSource
	,PastDueAndCurrentCOs.HistoryTransactionType
	,PastDueAndCurrentCOs.DemandSupplyType
	,PastDueAndCurrentCOs.OrderType
	,PastDueAndCurrentCOs.ComponentLineType
	,PastDueAndCurrentCOs.DemandType
	,PastDueAndCurrentCOs.ItemControllingNetUnitPrice
	,PastDueAndCurrentCOs.CORemainRequiredQuantity
	,PastDueAndCurrentCOs.CORemainingRequiredAmount
	,PastDueAndCurrentCOs.IssuedQuantity
	,PastDueAndCurrentCOs.COReversedQuantity
	,PastDueAndCurrentCOs.NetShippedQuantity
	,PastDueAndCurrentCOs.ShippedAmount
	,PastDueAndCurrentCOs.COReversedAmount
	,PastDueAndCurrentCOs.NetShippedAmount
	,PastDueAndCurrentCOs.TotalSalesQuantity
	,PastDueAndCurrentCOs.ViewSource AS Expr1
	,(
		CASE 
			WHEN ViewSource = 'Shipments'
				THEN NetShippedAmount - (TotalRolledCost * NetShippedQuantity)
			ELSE CORemainingRequiredAmount - (TotalRolledCost * CORemainRequiredQuantity)
			END
		) AS MarginAmount
	,PastDueAndCurrentCOs.TotalSalesAmount
	,PastDueAndCurrentCOs.Labor
	,PastDueAndCurrentCOs.VOH
	,PastDueAndCurrentCOs.FOH
	,PastDueAndCurrentCOs.Material
	,PastDueAndCurrentCOs.ItemClass4
	,PastDueAndCurrentCOs.ItemClass5
	,PastDueAndCurrentCOs.ItemClass6
	,PastDueAndCurrentCOs.GroupTechnologyCode
	,_CAP_Business.CodeDescription
	,PastDueAndCurrentCOs.CostType
	,PastDueAndCurrentCOs.RolledMaterialCost
	,PastDueAndCurrentCOs.RolledLaborCost
	,PastDueAndCurrentCOs.RolledVariableOverheadCost
	,PastDueAndCurrentCOs.RolledFixedOverheadCost
	,PastDueAndCurrentCOs.Inventory
	,PastDueAndCurrentCOs.InventoryAccount
	,PastDueAndCurrentCOs.CustomerID
	,PastDueAndCurrentCOs.PointOfUseID
	,PastDueAndCurrentCOs.CustomerName
	,PastDueAndCurrentCOs.CONumber
	,PastDueAndCurrentCOs.LineStatus
	,PastDueAndCurrentCOs.IsDemandFirm_LineType
	,PastDueAndCurrentCOs.CSR
	,_CAP_Class_Ref.ITEM_REF1_DESC
	,_CAP_Class_Ref.BUSINESS_TEAM
	,_CAP_Class_Ref.BT_DESCRIPTION
	,_CAP_MarketSegment.CodeDescription AS Mkt
	,_CAP_Industry.CodeDescription AS Ind
	,_CAP_Main_Commodity.MainCommodityCodeA
	,_CAP_Main_Commodity.MainCommodityCodeB
	,_CAP_Main_Commodity.MainCommodityCode
	,_CAP_Main_Commodity.MainCommodity
	,PastDueAndCurrentCOs.DemandItemKey
	,PastDueAndCurrentCOs.SupplyItemKey
	,PastDueAndCurrentCOs.DemandSupplyKey
	,PastDueAndCurrentCOs.COHeaderKey
	,PastDueAndCurrentCOs.CustomerKey
FROM (
	SELECT _NoLock_FS_Item.ItemNumber
		,DemandSupply.ItemKey
		,_NoLock_FS_Item.ItemDescription
		,_NoLock_FS_Item.MakeBuyCode
		,_NoLock_FS_Item.ItemReference1
		,_NoLock_FS_Item.ItemClass4
		,_NoLock_FS_Item.ItemClass5
		,_NoLock_FS_Item.ItemClass6
		,_NoLock_FS_Item.GroupTechnologyCode
		,_NoLock_FS_ItemCost.CostType
		,_NoLock_FS_ItemCost.RolledMaterialCost
		,_NoLock_FS_ItemCost.RolledLaborCost
		,_NoLock_FS_ItemCost.RolledVariableOverheadCost
		,_NoLock_FS_ItemCost.RolledFixedOverheadCost
		,_NoLock_FS_ItemCost.TotalRolledCost
		,_NoLock_FS_ItemData.ExternalWIPQuantity + _NoLock_FS_ItemData.InShippingQuantity + _NoLock_FS_ItemData.InInspectionQuantity + _NoLock_FS_ItemData.InternalWIPQuantity + _NoLock_FS_ItemData.OnHandQuantity AS Inventory
		,_NoLock_FS_Customer.CustomerID
		,_NoLock_FS_Customer.CustomerName
		,_NoLock_FS_Item.InventoryAccount
		,(
			SELECT TOP (1) CAST(CONVERT(VARCHAR, YEAR(GETDATE())) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME) AS NewPeriodEndDate
			FROM _NoLock_FS_FiscalPeriod
			WHERE (YEAR(GETDATE()) = CONVERT(INT, FiscalYear))
				AND (GETDATE() <= CAST(CONVERT(VARCHAR, YEAR(GETDATE())) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME))
			ORDER BY FiscalPeriod
			) AS CurrentPeriodEndDate
		,(
			SELECT TOP (1) FiscalYear
			FROM _NoLock_FS_FiscalPeriod AS _NoLock_FS_FiscalPeriod_5
			WHERE (YEAR(GETDATE()) = CONVERT(INT, FiscalYear))
				AND (GETDATE() <= CAST(CONVERT(VARCHAR, YEAR(GETDATE())) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME))
			ORDER BY FiscalPeriod
			) AS CurrentFiscalYear
		,(
			SELECT TOP (1) FiscalPeriod
			FROM _NoLock_FS_FiscalPeriod AS _NoLock_FS_FiscalPeriod_4
			WHERE (YEAR(GETDATE()) = CONVERT(INT, FiscalYear))
				AND (GETDATE() <= CAST(CONVERT(VARCHAR, YEAR(GETDATE())) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME))
			ORDER BY FiscalPeriod
			) AS CurrrentFiscalPeriod
		,DemandSupply.RequiredDate
		,ISNULL((
				SELECT TOP (1) CAST(CONVERT(VARCHAR, YEAR(DemandSupply.RequiredDate)) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME) AS NewPeriodEndDate
				FROM _NoLock_FS_FiscalPeriod AS _NoLock_FS_FiscalPeriod_3
				WHERE (YEAR(DemandSupply.RequiredDate) = CONVERT(INT, FiscalYear))
					AND (DemandSupply.RequiredDate <= CAST(CONVERT(VARCHAR, YEAR(DemandSupply.RequiredDate)) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME))
				ORDER BY FiscalPeriod
				), '01/01/2079') AS RequiredDatePeriodEndDate
		,ISNULL((
				SELECT TOP (1) FiscalYear
				FROM _NoLock_FS_FiscalPeriod AS _NoLock_FS_FiscalPeriod_2
				WHERE (YEAR(DemandSupply.RequiredDate) = CONVERT(INT, FiscalYear))
					AND (DemandSupply.RequiredDate <= CAST(CONVERT(VARCHAR, YEAR(DemandSupply.RequiredDate)) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME))
				ORDER BY FiscalPeriod
				), '2079') AS RequiredDateFiscalYear
		,ISNULL((
				SELECT TOP (1) FiscalPeriod
				FROM _NoLock_FS_FiscalPeriod AS _NoLock_FS_FiscalPeriod_1
				WHERE (YEAR(DemandSupply.RequiredDate) = CONVERT(INT, FiscalYear))
					AND (DemandSupply.RequiredDate <= CAST(CONVERT(VARCHAR, YEAR(DemandSupply.RequiredDate)) + '-' + CONVERT(VARCHAR, MONTH(PeriodEndDate)) + '-' + CONVERT(VARCHAR, DAY(PeriodEndDate)) AS DATETIME))
				ORDER BY FiscalPeriod
				), '12') AS RequiredDateFiscalPeriod
		,DemandSupply.ViewSource
		,DemandSupply.HistoryTransactionType
		,DemandSupply.DemandSupplyType
		,DemandSupply.OrderType
		,DemandSupply.ComponentLineType
		,DemandSupply.DemandType
		,CASE 
			WHEN DemandSupply.DemandSupplyType = 'D'
				THEN DemandSupply.AccountOrWorkOrderNumber
			ELSE ''
			END AS CONumber
		,DemandSupply.LineStatus
		,CASE 
			WHEN (
					DemandSupply.DemandSupplyType = 'S'
					AND ViewSource = 'OpenOrders'
					)
				THEN isnull(TotalRolledCost, 0)
			ELSE isnull(DemandSupply.ItemControllingNetUnitPrice, 0)
			END AS ItemControllingNetUnitPrice
		,ISNULL(DemandSupply.CORemainRequiredQuantity, 0) AS CORemainRequiredQuantity
		,ISNULL(DemandSupply.CORemainingRequiredAmount, 0) AS CORemainingRequiredAmount
		,ISNULL(DemandSupply.IssuedQuantity, 0) AS IssuedQuantity
		,ISNULL(DemandSupply.COReversedQuantity, 0) AS COReversedQuantity
		,ISNULL(DemandSupply.NetCOShippedQuantity, 0) AS NetShippedQuantity
		,ISNULL(DemandSupply.COShippedAmount, 0) AS ShippedAmount
		,ISNULL(DemandSupply.COReversedAmount, 0) AS COReversedAmount
		,ISNULL(DemandSupply.NetCOShippedAmount, 0) AS NetShippedAmount
		,ISNULL(DemandSupply.CORemainRequiredQuantity, 0) + ISNULL(DemandSupply.NetCOShippedQuantity, 0) AS TotalSalesQuantity
		,ISNULL(DemandSupply.CORemainingRequiredAmount, 0) + ISNULL(DemandSupply.NetCOShippedAmount, 0) AS TotalSalesAmount
		,CASE 
			WHEN DemandSupplyType = 'D'
				THEN RolledLaborCost * (isnull(DemandSupply.CORemainRequiredQuantity, 0) + ISNULL(DemandSupply.NetCOShippedQuantity, 0))
			END AS Labor
		,CASE 
			WHEN DemandSupplyType = 'D'
				THEN RolledVariableOverheadCost * (isnull(DemandSupply.CORemainRequiredQuantity, 0) + ISNULL(DemandSupply.NetCOShippedQuantity, 0))
			END AS VOH
		,CASE 
			WHEN DemandSupplyType = 'D'
				THEN RolledFixedOverheadCost * (isnull(DemandSupply.CORemainRequiredQuantity, 0) + ISNULL(DemandSupply.NetCOShippedQuantity, 0))
			END AS FOH
		,CASE 
			WHEN DemandSupplyType = 'D'
				THEN RolledMaterialCost * (isnull(DemandSupply.CORemainRequiredQuantity, 0) + ISNULL(DemandSupply.NetCOShippedQuantity, 0))
			END AS Material
		,NULL AS IsDemandFirm_LineType
		,DemandSupply.DemandItemKey
		,DemandSupply.SupplyItemKey
		,DemandSupply.PointOfUseID
		,DemandSupply.DemandSupplyKey
		,DemandSupply.COHeaderKey
		,_NoLock_FS_Customer.CustomerKey
		,_NoLock_FS_COHeader_1.CSR
	FROM _NoLock_FS_ItemData
	RIGHT OUTER JOIN _NoLock_FS_Item ON _NoLock_FS_ItemData.ItemKey = _NoLock_FS_Item.ItemKey
	LEFT OUTER JOIN _NoLock_FS_ItemCost ON _NoLock_FS_Item.ItemKey = _NoLock_FS_ItemCost.ItemKey
	RIGHT OUTER JOIN (
		--START OPEN ORDERS                                                        
		SELECT CASE 
				WHEN (_NoLock_FS_DemandSupply_1.SupplyItemKey) IS NULL
					THEN _NoLock_FS_DemandSupply_1.DemandItemKey
				ELSE _NoLock_FS_DemandSupply_1.SupplyItemKey
				END AS ItemKey
			,'OpenOrders' AS ViewSource
			,_NoLock_FS_DemandSupply_1.DemandSupplyType
			,_NoLock_FS_DemandSupply_1.OrderType
			,_NoLock_FS_DemandSupply_1.DemandType
			,NULL AS HistoryTransactionType
			,NULL AS LineType
			,_NoLock_FS_DemandSupply_1.ComponentLineType
			,_NoLock_FS_DemandSupply_1.LineStatus
			,_NoLock_FS_DemandSupply_1.OperationSequenceNumber AS COLineNumber
			,_NoLock_FS_DemandSupply_1.RequiredDate
			,_NoLock_FS_DemandSupply_1.RequiredQuantity AS DSRequiredQuantity
			,NULL AS IssuedQuantity
			,NULL AS ReceiptQuantity
			,NULL AS COReversedQuantity
			,NULL AS NetCOShippedQuantity
			,NULL AS COShippedAmount
			,NULL AS COReversedAmount
			,NULL AS NetCOShippedAmount
			,CASE 
				WHEN _NoLock_FS_DemandSupply_1.DemandSupplyType = 'D'
					THEN ISNULL(_NoLock_FS_DemandSupply_1.RequiredQuantity, 0) - ISNULL(_NoLock_FS_DemandSupply_1.IssuedQuantity, 0)
				ELSE 0
				END AS CORemainRequiredQuantity
			,CASE 
				WHEN _NoLock_FS_DemandSupply_1.DemandSupplyType = 'D'
					THEN (ISNULL(_NoLock_FS_DemandSupply_1.RequiredQuantity, 0) - ISNULL(_NoLock_FS_DemandSupply_1.IssuedQuantity, 0)) * ISNULL(_NoLock_FS_DemandData.ItemControllingNetUnitPrice, 0)
				ELSE 0
				END AS CORemainingRequiredAmount
			,_NoLock_FS_DemandSupply_1.LastReceiptDate
			,CASE 
				WHEN _NoLock_FS_DemandSupply_1.DemandSupplyType = 'D'
					THEN _NoLock_FS_COHeader.CONumber
				ELSE _NoLock_FS_MOHeader.MONumber
				END AS AccountOrWorkOrderNumber
			,_NoLock_FS_DemandSupply_1.PointOfUseID
			,_NoLock_FS_DemandSupply_1.DemandItemKey
			,_NoLock_FS_DemandSupply_1.SupplyItemKey
			,_NoLock_FS_DemandSupply_1.COHeaderKey
			,CASE 
				WHEN _NoLock_FS_DemandSupply_1.DemandSupplyType <> 'D'
					THEN (
							SELECT TOP (1) _NoLock_FS_ItemCost.TotalRolledCost
							FROM _NoLock_FS_ItemCost
							WHERE _NoLock_FS_DemandSupply_1.SupplyItemKey = _NoLock_FS_ItemCost.ItemKey
								AND (_NoLock_FS_ItemCost.CostType = '0')
							)
				ELSE ISNULL(_NoLock_FS_DemandData.ItemControllingNetUnitPrice, 0)
				END AS ItemControllingNetUnitPrice
			,_NoLock_FS_DemandSupply_1.DemandSupplyKey
		FROM _NoLock_FS_DemandSupply AS _NoLock_FS_DemandSupply_1
		LEFT OUTER JOIN _NoLock_FS_MOHeader ON _NoLock_FS_DemandSupply_1.MOHeaderKey = _NoLock_FS_MOHeader.MOHeaderKey
		LEFT OUTER JOIN _NoLock_FS_COHeader ON _NoLock_FS_DemandSupply_1.COHeaderKey = _NoLock_FS_COHeader.COHeaderKey
		LEFT OUTER JOIN _NoLock_FS_DemandData ON _NoLock_FS_DemandSupply_1.DemandSupplyKey = _NoLock_FS_DemandData.DemandKey
		WHERE (
				_NoLock_FS_DemandSupply_1.OrderType IN (
					'C'
					,'F'
					)
				)
			AND (_NoLock_FS_DemandSupply_1.ComponentLineType = 'N')
			AND (_NoLock_FS_DemandSupply_1.ComponentLineType <> 'B')
			AND (_NoLock_FS_DemandSupply_1.RequiredDate <= @EndDate)
			AND (_NoLock_FS_DemandSupply_1.LineStatus < '5')
			AND (ISNULL(_NoLock_FS_DemandSupply_1.RequiredQuantity, 0) - ISNULL(_NoLock_FS_DemandSupply_1.ReceiptIssuedQuantity, 0) > '0')
		
		UNION ALL
		
		--END OPEN ORDERS		
		
		--START SHIPMENTS
			SELECT TOP (100) PERCENT _NoLock_FS_ItemHistoryLink.ItemKey
				,'Shipments' AS ViewSource
				,'D' AS DemandSupplyType
				,_NoLock_FS_HistoryShipment.OrderType
				,_NoLock_FS_HistoryShipment.ShipmentTransactionType AS DemandType
				,_NoLock_FS_HistoryShipment.IssueType
				,_NoLock_FS_HistoryShipment.COLineType
				,NULL AS ComponentLineType
				,MAX(DISTINCT _NoLock_FS_HistoryShipment.COLineStatus) AS COLineStatus
				,_NoLock_FS_HistoryShipment.COLineNumber
				,_NoLock_FS_HistoryShipment.TransactionDate AS RequiredDate
				,NULL AS DSRequiredQuantity
				,SUM(ISNULL(_NoLock_FS_HistoryShipment.ShippedQuantity, 0)) AS IssuedQuantity
				,NULL AS ReceiptQuantity
				,SUM(ISNULL(_NoLock_FS_HistoryShipment.ReversedQuantity, 0)) AS COReversedQuantity
				,SUM(ISNULL(_NoLock_FS_HistoryShipment.ShippedQuantity, 0) - ISNULL(_NoLock_FS_HistoryShipment.ReversedQuantity, 0)) AS NetCOShippedQuantity
				,SUM(ISNULL(_NoLock_FS_HistoryShipment.ShippedQuantity, 0) * ISNULL(_NoLock_FS_HistoryShipment.ItemControllingNetUnitPrice, 0)) AS COShippedAmount
				,SUM(ISNULL(_NoLock_FS_HistoryShipment.ReversedQuantity, 0) * ISNULL(_NoLock_FS_HistoryShipment.ItemControllingNetUnitPrice, 0)) AS COReversedAmount
				,SUM((ISNULL(_NoLock_FS_HistoryShipment.ShippedQuantity, 0) - ISNULL(_NoLock_FS_HistoryShipment.ReversedQuantity, 0)) * ISNULL(_NoLock_FS_HistoryShipment.ItemControllingNetUnitPrice, 0)) AS NetCOShippedAmount
				,NULL AS CORemainingRequiredQuantity
				,NULL AS CORemainingRequiredAmount
				,NULL AS LastReceiptDate
				,_NoLock_FS_HistoryShipment.CONumber AS AccountOrWorkOrderNumber
				,_NoLock_FS_HistoryShipment.CustomerID AS PointOfUseID
				,_NoLock_FS_ItemHistoryLink.ItemKey AS DemandItemKey
				,NULL AS SupplyItemKey
				,NULL AS COHeaderKey
				,_NoLock_FS_HistoryShipment.ItemControllingNetUnitPrice
				,_NoLock_FS_HistoryShipment.HistoryShipmentKey AS DemandSupplyKey
			FROM _NoLock_FS_ItemHistoryLink
			INNER JOIN _NoLock_FS_HistoryShipment ON _NoLock_FS_ItemHistoryLink.ItemHistoryKey = _NoLock_FS_HistoryShipment.HistoryShipmentKey
			WHERE (
					_NoLock_FS_HistoryShipment.TransactionDate BETWEEN @StartDate
						AND @EndDate
					)
			GROUP BY _NoLock_FS_ItemHistoryLink.ItemKey
				,_NoLock_FS_HistoryShipment.OrderType
				,_NoLock_FS_HistoryShipment.COLineType
				,_NoLock_FS_HistoryShipment.ShipmentTransactionType
				,_NoLock_FS_HistoryShipment.COLineNumber
				,_NoLock_FS_HistoryShipment.PromisedShipDate
				,_NoLock_FS_HistoryShipment.IssueType
				,_NoLock_FS_HistoryShipment.ItemControllingNetUnitPrice
				,_NoLock_FS_HistoryShipment.CONumber
				,_NoLock_FS_HistoryShipment.CustomerID
				,_NoLock_FS_HistoryShipment.ShipToCustomerID
				,_NoLock_FS_HistoryShipment.HistoryShipmentKey
				,_NoLock_FS_HistoryShipment.TransactionDate
			--END SHIPMENTS
		) AS DemandSupply ON _NoLock_FS_Item.ItemKey = DemandSupply.ItemKey
	LEFT OUTER JOIN _NoLock_FS_Customer
	INNER JOIN _NoLock_FS_COHeader AS _NoLock_FS_COHeader_1 ON _NoLock_FS_Customer.CustomerKey = _NoLock_FS_COHeader_1.CustomerKey ON DemandSupply.COHeaderKey = _NoLock_FS_COHeader_1.COHeaderKey WHERE (_NoLock_FS_ItemCost.CostType IN (0))
		AND (
			_NoLock_FS_Item.InventoryAccount = (
				CASE 
					WHEN (DemandSupply.DemandSupplyType = 'S')
						THEN '1-00-1240'
					ELSE (_NoLock_FS_Item.InventoryAccount)
					END
				)
			)
	) AS PastDueAndCurrentCOs
LEFT OUTER JOIN _CAP_Main_Commodity ON PastDueAndCurrentCOs.GroupTechnologyCode = _CAP_Main_Commodity.MainCommodityCode
LEFT OUTER JOIN _CAP_Industry ON PastDueAndCurrentCOs.ItemClass5 = _CAP_Industry.IndustryCode
LEFT OUTER JOIN _CAP_MarketSegment ON PastDueAndCurrentCOs.ItemClass4 = _CAP_MarketSegment.SegmentCode
LEFT OUTER JOIN _CAP_Business ON PastDueAndCurrentCOs.ItemClass6 = _CAP_Business.BusinessCode
LEFT OUTER JOIN _CAP_Class_Ref ON PastDueAndCurrentCOs.ItemReference1 = _CAP_Class_Ref.ITEM_REF1
