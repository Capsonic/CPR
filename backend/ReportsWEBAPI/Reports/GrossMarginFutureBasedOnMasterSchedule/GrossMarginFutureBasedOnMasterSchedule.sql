-- Gross Margin Future based on Master Schedules
--Question: Gross Margin uses ItemControllingUnitPrice instead of CostType1

--declare @startDate as datetime = '2017-04-03'
--declare @currentPeriodEndDate as datetime = '2017-04-29'
--declare @secondPeriodEndDate as datetime = '2017-06-03'
--declare @thirdPeriodEndDate as datetime = '2017-07-01'

--select @startDate
--select DATEADD(d,1,@startdate)

--select @startDate =[FSDBMR].[dbo].[_CAP_GetFirstDayOfWeek](GETDATE()) + 1;

SELECT 

	MAX(DISTINCT I.ItemKey) AS ItemKey
	,MAX(DISTINCT I.ItemReference1) AS ItemRef1
	,I.ItemNumber AS ITEM
	,I.ItemDescription AS ItemDesc

	,FS_ItemData.ExternalWIPQuantity + FS_ItemData.InShippingQuantity + 
	FS_ItemData.InInspectionQuantity + FS_ItemData.InternalWIPQuantity + FS_ItemData.OnHandQuantity AS Inventory

	,_CAP_FS_ItemCost_Types.CostType0
	,_CAP_FS_ItemCost_Types.CostType1
	,MSCHD.D_SUM_OF_PAST AS PAST_DUE_QTY
	,MSCHD.D_SUM_OF_PAST * _CAP_FS_ItemCost_Types.CostType1 AS PAST_DUE_AMOUNT
			
	--,I.Buyer AS Buyr
	--,I.Planner AS Plnr
	--,I.InventoryAccount AS InvAcct
	--,I.MakeBuyCode AS MB	
	
	--,I.PackageType AS Packaging
	--,I.PiecesPerPackage AS PiecesPer
	--,I.RunLeadTimeDays AS RunLeadTime
	--,I.FixedLeadTimeDays AS FixedLeadTime
	--,I.InspectionLeadTimeDays AS InspLeadTime
	--,I.LotSizeMinimum AS LotSizeMin
	--,I.LotSizeMultiplier AS LotSizeMult
	--,I.PlanningFenceDays AS PlanningFence
	--,I.ForecastPeriod
	
	--,FS_ItemData.ATPQuantity
	--,FS_ItemData.ExternalWIPQuantity
	--,FS_ItemData.InShippingQuantity
	--,FS_ItemData.InInspectionQuantity
	--,FS_ItemData.InternalWIPQuantity
	--,FS_ItemData.OnHandQuantity
	--,FS_ItemData.OnHoldQuantity
	
	--,_CAP_Class_Ref.ITEM_REF1_DESC
	--,I.ItemClass1
	--,I.ItemClass2
	--,I.ItemClass3
	--,I.GroupTechnologyCode
	--,_CAP_Comodity_Codes.SubCommodity
	--,_CAP_Comodity_Codes.MainCommodity
	
	,MSCHD.D_ThroughW21 AS ThroughW21
	,MSCHD.D_ThroughW21 * _CAP_FS_ItemCost_Types.CostType1 AS ThroughW21_Amount
	
	,MSCHD.D_All_Available AS All_Available
	,MSCHD.D_All_Available * _CAP_FS_ItemCost_Types.CostType1 AS All_Available_Amount
	
	,MSCHD.CurrentPeriodQty AS CurrentPeriodQty
	,MSCHD.CurrentPeriodQty * _CAP_FS_ItemCost_Types.CostType1 AS CurrentPeriodAmount
	
	,MSCHD.SecondPeriodQty AS SecondPeriodQty
	,MSCHD.SecondPeriodQty * _CAP_FS_ItemCost_Types.CostType1 AS SecondPeriodAmount
	
	,MSCHD.ThirdPeriodQty AS ThirdPeriodQty
	,MSCHD.ThirdPeriodQty * _CAP_FS_ItemCost_Types.CostType1 AS ThirdPeriodAmount
	
	,TotalRolledCost


	,SHIPMENTS.NetShippedQuantity
	
	,SHIPMENTS.NetShippedAmount

	,SHIPMENTS.NetShippedAmount - (TotalRolledCost * SHIPMENTS.NetShippedQuantity) as MarginShipment

	,(MSCHD.CurrentPeriodQty * _CAP_FS_ItemCost_Types.CostType1) - (TotalRolledCost * MSCHD.CurrentPeriodQty) as MarginCO
	
	--,MSCHD.StartDate
FROM FS_Item AS I WITH (NOLOCK)
LEFT OUTER JOIN _CAP_FS_ItemCost_Types ON I.ItemKey = _CAP_FS_ItemCost_Types.ItemKey
LEFT OUTER JOIN FS_ItemData ON I.ItemKey = FS_ItemData.ItemKey
LEFT OUTER JOIN _CAP_Comodity_Codes ON I.GroupTechnologyCode = _CAP_Comodity_Codes.GroupTechnologyCode
LEFT OUTER JOIN _CAP_Class_Ref ON I.ItemReference1 = _CAP_Class_Ref.ITEM_REF1
RIGHT OUTER JOIN (
	SELECT ITEM
		,_ItemKey
		,@startDate AS StartDate
		,(
			SELECT ISNULL(SUM(D.ItemOrderedQuantity - D.InShippingQuantity), 0) AS Expr1
			FROM FS_Item AS I WITH (NOLOCK)
			LEFT OUTER JOIN FS_COLine AS D WITH (NOLOCK) ON I.ItemKey = D.ItemKey
			WHERE (I.ItemKey = A._ItemKey)
				AND (D.COLineStatus < '5')
			) AS D_All_Available
		,(
			SELECT ISNULL(SUM(D.ItemOrderedQuantity - D.InShippingQuantity), 0) AS Expr1
			FROM FS_Item AS I WITH (NOLOCK)
			LEFT OUTER JOIN FS_COLine AS D WITH (NOLOCK) ON I.ItemKey = D.ItemKey
			WHERE (I.ItemKey = A._ItemKey)
				AND (D.COLineStatus < '5')
				AND (D.PromisedShipDate <= @startDate + 147)
			) AS D_ThroughW21
		,--To later evaluate if an item has demands in the time period desired
		(
			SELECT ISNULL(SUM(D.ItemOrderedQuantity - D.InShippingQuantity), 0) AS Expr1
			FROM FS_Item AS I WITH (NOLOCK)
			LEFT OUTER JOIN FS_COLine AS D WITH (NOLOCK) ON I.ItemKey = D.ItemKey
			WHERE (I.ItemKey = A._ItemKey)
				AND (D.COLineStatus < '5')
				AND (D.PromisedShipDate < getdate())
				AND (D.PromisedShipDate > '1/1/2000')
			) AS D_SUM_OF_PAST
		--All demands due pior to Monday current week

		--Current Period
		,(
			SELECT ISNULL(SUM(D.ItemOrderedQuantity - D.InShippingQuantity), 0) AS Expr1
			FROM FS_Item AS I WITH (NOLOCK)
			LEFT OUTER JOIN FS_COLine AS D WITH (NOLOCK) ON I.ItemKey = D.ItemKey
			WHERE (I.ItemKey = A._ItemKey)
				AND (D.COLineStatus < '5')
				AND (D.PromisedShipDate <= @currentPeriodEndDate)
			) AS CurrentPeriodQty
		
		
		--Second Month
		,(
			SELECT ISNULL(SUM(D.ItemOrderedQuantity - D.InShippingQuantity), 0) AS Expr1
			FROM FS_Item AS I WITH (NOLOCK)
			LEFT OUTER JOIN FS_COLine AS D WITH (NOLOCK) ON I.ItemKey = D.ItemKey
			WHERE (I.ItemKey = A._ItemKey)
				AND (D.COLineStatus < '5')
				AND (D.PromisedShipDate > @currentPeriodEndDate)
				AND (D.PromisedShipDate <= @secondPeriodEndDate)
			) AS SecondPeriodQty
		
		
		--Third Month
		,(
			SELECT ISNULL(SUM(D.ItemOrderedQuantity - D.InShippingQuantity), 0) AS Expr1
			FROM FS_Item AS I WITH (NOLOCK)
			LEFT OUTER JOIN FS_COLine AS D WITH (NOLOCK) ON I.ItemKey = D.ItemKey
			WHERE (I.ItemKey = A._ItemKey)
				AND (D.COLineStatus < '5')
				AND (D.PromisedShipDate > @secondPeriodEndDate)
				AND (D.PromisedShipDate <= @thirdPeriodEndDate)
			) AS ThirdPeriodQty
			
						
			,(SELECT  top(1) _NoLock_FS_ItemCost.TotalRolledCost FROM      _NoLock_FS_ItemCost join FS_Item on FS_Item.ItemKey = _NoLock_FS_ItemCost.ItemKey
				where FS_Item.ItemKey = A._ItemKey and _NoLock_FS_ItemCost.CostType = '0') AS TotalRolledCost
				
				
		
		
	FROM (
		SELECT DISTINCT IM.ItemNumber AS ITEM
			,DS.ItemKey AS _ItemKey
		FROM FS_Item AS IM WITH (NOLOCK)
		INNER JOIN FS_COLine AS DS WITH (NOLOCK) ON DS.ItemKey = IM.ItemKey
		WHERE (DS.COLineStatus < '5')
		) AS A
	GROUP BY _ItemKey
		,ITEM
	) MSCHD ON I.ItemKey = MSCHD._ItemKey
	
	LEFT OUTER JOIN 
	
	(SELECT  
		_NoLock_FS_ItemHistoryLink.ItemKey
		,SUM(ISNULL(_NoLock_FS_HistoryShipment.ShippedQuantity, 0) - ISNULL(_NoLock_FS_HistoryShipment.ReversedQuantity, 0)) AS NetShippedQuantity
		
		,SUM((ISNULL(_NoLock_FS_HistoryShipment.ShippedQuantity, 0) - ISNULL(_NoLock_FS_HistoryShipment.ReversedQuantity, 0))
		 * ISNULL(_NoLock_FS_HistoryShipment.ItemControllingNetUnitPrice, 0))	AS NetShippedAmount
		 
		FROM _NoLock_FS_ItemHistoryLink
					INNER JOIN _NoLock_FS_HistoryShipment ON _NoLock_FS_ItemHistoryLink.ItemHistoryKey = _NoLock_FS_HistoryShipment.HistoryShipmentKey
		WHERE (
				_NoLock_FS_HistoryShipment.TransactionDate BETWEEN @StartDate
					AND @thirdPeriodEndDate
				) 
				--and _NoLock_FS_ItemHistoryLink.ItemKey = 22428
		GROUP BY _NoLock_FS_ItemHistoryLink.ItemKey) SHIPMENTS ON SHIPMENTS.ItemKey = I.ItemKey
	
	
GROUP BY I.ItemNumber
	,I.Buyer
	,I.Planner
	,I.InventoryAccount
	,I.MakeBuyCode
	,I.ItemDescription
	,I.PackageType
	,I.PiecesPerPackage
	,I.RunLeadTimeDays
	,I.FixedLeadTimeDays
	,I.InspectionLeadTimeDays
	,I.LotSizeMinimum
	,I.LotSizeMultiplier
	,I.PlanningFenceDays
	,I.ForecastPeriod
	,FS_ItemData.ATPQuantity
	,FS_ItemData.ExternalWIPQuantity
	,FS_ItemData.InShippingQuantity
	,FS_ItemData.InInspectionQuantity
	,FS_ItemData.InternalWIPQuantity
	,FS_ItemData.OnHandQuantity
	,FS_ItemData.OnHoldQuantity
	,_CAP_FS_ItemCost_Types.CostType0
	,_CAP_FS_ItemCost_Types.CostType1
	,_CAP_Class_Ref.ITEM_REF1_DESC
	,I.ItemClass1
	,I.ItemClass2
	,I.ItemClass3
	,I.GroupTechnologyCode
	,_CAP_Comodity_Codes.SubCommodity
	,_CAP_Comodity_Codes.MainCommodity
	,MSCHD.D_SUM_OF_PAST
	,MSCHD.D_ThroughW21
	,MSCHD.D_All_Available
	,MSCHD.CurrentPeriodQty
	,MSCHD.SecondPeriodQty
	,MSCHD.ThirdPeriodQty	
	,TotalRolledCost
	,SHIPMENTS.NetShippedQuantity
	,SHIPMENTS.NetShippedAmount
	--,MSCHD.StartDate
	
--HAVING      (I.InventoryAccount = '1-00-1240')

--having ItemNumber = '50350'

ORDER BY I.ItemNumber
