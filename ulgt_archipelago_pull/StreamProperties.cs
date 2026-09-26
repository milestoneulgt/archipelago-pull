using System.Text.Json;

namespace ulgtArchipelagoPull;

public class PropertyResponse
{
    public string? streamId { get; set; }
    public List<object>? properties { get; set; }
    public int totalCount { get; set; }
}

public class StreamProperties
{
    /*
 GraphQL query to fetch stream properties

 Sections In Order:
  1) Building location and basic information
  2) Hazards
  3) Valuations and modeling
      Added Fields (Replaces businessInterruptionInsuredValue and businessInterruptionInsuredValueDisplay)
        businessInterruptionRedundancy
        businessInterruptionCostDisplay 
        businessInterruptionPreparedness 
        businessInterruptionCost

  4) Stewardship and occupancy
  5) Building characteristics,construction, and envelope
  6) Fire and flood protection
  7) Administrative
      attrbuteProvenance is not a String its an object
      ownerAttributes is not a String its an object 
*/
    private readonly string stream_properties_query = $$"""
        query {
            streamProperties(input: {
            streamID:"{0}",            
            offset:0, 
            limit:{{PropertyHelpers.pageSize}}
            }) 
            {
                properties {
                    archipelagoID                                       
                    buildingDescription
                    fireProtectionDescription
                    securityDescription
                    city
                    contingency
                    country
                    county
                    geoCodeType
                    latitude
                    location
                    locationID
                    locationName
                    longitude
                    originalAddress
                    postalCode
                    propertyName
                    propertyStatus
                    region                        
                    state
                    streetAddress
                    customerProvidedGeocode
                    yearBuilt
                    yearLastUpgraded
                    yearsInPortfolio
    
                    baseFloodElevation
                    commodityHazardClass
                    contentsVulnerabilityWind
                    convectiveStormZone
                    exclusions
                    equipmentEarthquakeBracing                    
                    distanceToCoast
                    distanceToNearestFault
                    floodMissiles
                    floodZone
                    archipelagoFloodZone        
                    hailHazardClass
                    hazardSummary
                    landslideSusceptibility
                    lightningHazardClass
                    liquefactionSusceptibility
                    seismicDesignQuality
                    seismicDesignRValue
                    seismicHazardClass
                    seismicInspectionDate
                    seismicStatus
                    seismicZone
                    archipelagoSeismicZone
                    siteSoilClassification
                    sprinklerPipesSeismicallyBraced
                    tornadoHazardClass
                    tsunamiHazardClass
                    wildfireHazardClass
                    wildfireZone
                    windHazardClass
                    windMissiles
                    windZone
                    archipelagoWindZone
    
                    annualBaseRent
                    annualGrossProfit
                    annualServiceCharge  
                    
                    buildingReplacementCost
                    buildingReplacementCostDisplay
                    buildingReplacementCostPartner
                    buildingReplacementCostPercentDifference
                    buildingReplacementCostPercentDifferenceRange
                    buildingReplacementCostValueDifference, 
                    buildingValue
                    businessInterruptionRedundancy
                    businessInterruptionCostDisplay 
                    businessInterruptionPreparedness 
                    businessInterruptionCost
                    businessIncomeValue
                    contentsReplacementCost
                    contentsReplacementCostDisplay
                    contingentBusinessInterruptionFlag
                    contingentBusinessInterruptionComments
                    dependencyFlag
                    dependencyComments
                    dependencyCoveragePercentage
                    dependencyValue
                    effectiveFrom
                    electronicsValue
                    exchangeRate
                    exchangeRateDate
                    extraExpenseValue
                    fineArtJewelryValue
                    fireEstimatedMaximumLoss
                    fireProbableMaximumLoss
                    improvementsValue
                    inventoryValue
                    lastValuationDate
                    machineryValue
                    miscBusinessInterruptionValue                
                    miscBuildingReplacementCostValue
                    miscContentsValue
                    modelingAvailable
                    otherContentsValue
                    payrollValue
                    quakeScenarioEstimatedLoss
                    quakeScenarioUpperLoss
                    rentAndServiceIndemnityPeriod
                    replacementCostMethodology
                    replacementCostPerFloorAreaPartner
                    replacementCostPerFloorAreaPartnerDifference
                    replacementCostPerSquareFootage
                    replacementCostPerSquareFootageDisplay
                    stockThroughputFactor
                    stockThroughputInventoryValue
                    stockThroughputExcessInventoryValue
                    totalInsuredValue
                    totalInsuredValueDisplay
                    valuationCurrency

                    annualEstimatedRent
                    annualLossOfRevenue                  
                    acquiredOrBuilt
                    acquisitionOrOccupancyDate
                    businessContinuityPlans
                    controlOfCombustiblesProgram
                    dispositionDate                 
                    fireProtectionInspectionProgram
                    fireRatingForRoofCovering
                    fireRatingForWallSiding
                    firewiseCommunityParticipation
                    flammableLiquidGasLineManagement
                    flammableLiquidDescription
                    flammableLiquidStorageLocation
                    flammableLiquidCapacity
                    flammableLiquidsOnsite
                    flammableLiquidProgram
                    generatorTestingProgram
                    hasSeismicInspectionReport
                    hotWorkProgram
                    lastEngineeringVisitDate
                    leasedOrOwned
                    lossEngineeringReportPresent
                    multiTenant
                    occupancyDescription
                    occupancyType
                    occupancyTypeATC
                    occupancyTypeNAICS
                    occupancyTypeSIC
                    orgName
                    organizationLevelOne
                    organizationLevelTwo
                    organizationLevelThree
                    organizationLevelFour
                    owner                   
                    portableFireExtinguisherProgram
                    portfolio
                    primaryTenantNAICS
                    propertyManager
                    roofInspectionProgram
                    smokingControlsProgram
                    specificOccupancy
                    valueInspectionProgram
                    yearBuilt
                    yearsInPortfolio
                 
                    accessoryStructures
                    backupGenerator
                    backupGeneratorDieselStorageCapacity
                    backupGeneratorLocation                           
                    basementFinishType
                    buildingCondition
                    buildingExteriorOpening
                    buildingFootprintClass
                    buildingFoundationType
                    buildingRedundancy
                    chargingStationPower
                    chimneys
                    constructionDescription
                    constructionQuality
                    constructionType
                    constructionTypeAIR
                    constructionTypeATC
                    constructionTypeArchipelago
                    constructionTypeCluster
                    constructionTypeISO
                    constructionTypeRMS                 
                    crippleWalls
                    deck
                    defensibleSpace
                    distanceToNearestBuilding
                    doorAndFrameType
                    elevationOfGroundOrBasementLevelMEPEquipment                  
                    eqSpecialSystems
                    equipmentEarthquakeBracing
                    exteriorFuelStorage
                    fencesWithinFiveFeet
                    fireDetectionSystems
                    firePumpFlowRate
                    firePumpPowerSupply
                    firePumpBackupPeriod
                    firePumpTestResults
                    firePumpChurnRate
                    firePumpExcessCapacity
                    firstFloorElevation
                    firstFloorHeight                    
                    floorArea
                    floorOfInterest                   
                    floorSystem
                    foundationToFrameConnection
                    glassPercentage
                    glassType
                    heightOfRackStorage
                    highHazardAreaSprinklerType
                    hydrantFlowTestResultsStatic
                    hydrantFlowTestResultsResidual
                    inRackSprinklerType
                    interiorPartitions                    
                    naturalGasAutomaticShutoff
                    naturalGasPipesSeismicallyBraced
                    numberOfBuildings
                    numberOfFireHazardAreas
                    numberOfHighPriorityRecommendations
                    numberOfStoriesAboveGround
                    numberOfStoriesBelowGround
                    numberOfUnits
                    ornamentation
                    rackStoragePresent
                    residentialGarage
                    roofChimneysAnchorage
                    roofCopingAndFlashing
                    roofDeckAnchorage
                    roofDeckType
                    roofDescription
                    roofDrainageType
                    roofEquipmentAnchorage
                    roofEstimatedReplacementYear
                    roofGeometry
                    roofHailMitigation
                    roofImage
                    roofInstallYear
                    roofParapets
                    roofOverhang
                    roofScreensSignsAnchorageandBracing
                    roofSolarPanelCoverage
                    roofSolarPanelDescription
                    roofSolarPanelOwnership
                    roofStructures
                    roofSystem
                    roofSystemAnchorage
                    roofVent
                    roofingMaterialAnchorage
                    rooftopSolarPanels
                    rooftopWaterTanks
                    seismicDesignQuality
                    seismicDesignRValue
                    seismicStatus
                    shortColumnConcrete
                    sprinklerHeadSize
                    sprinklerHeadTemperatureRating
                    soffit
                    softStory
                    solarPanelPower
                    storageArrangementDescription
                    structuralDescription
                    structuralSystemUpgraded
                    structuralUpgradeType
                    surfaceRoughnessWind
                    torsion
                    treeExposure
                    verticalIrregularity
                    wallCladdingSystem
                    waterHeaterBracing
                    waterSupplyFlowRate
                    waterSupplyTime
                    windowProtection
                    windowType
                    yearLastUpgraded
                    


                    buildingSprinklered
                    buildingSprinklerType
                    contentsVulnerabilityFlood
                    contentsVulnerabilityVerticalDistributionOfContents
                    contentsVulnerabilityWind
                    emergencyFloodProtectionMeasures
                    percentageSprinklered
                    permanentFloodMeasuresPresent
                    remoteMonitoringOfSprinklerSystem
                    sprinklerLeakDetectionSystem
                    suitabilityOfFireProtectionMeasures

                    attributeProvenance {                    
                        sources                        
                        externalSourceURLs
                    }
                    comment
                    enriched   
                    ownerAttributes {
                        ownerText1
                        ownerText2
                        ownerText3
                        ownerText4
                        ownerText5
                        ownerText6
                        ownerText7
                        ownerText8
                        ownerText9
                        ownerText10
                        ownerText11
                        ownerText12
                        ownerText13
                        ownerText14
                        ownerText15
                        ownerDecimal1
                        ownerDecimal2
                        ownerDecimal3
                        ownerDecimal4
                        ownerDecimal5
                        ownerDecimal6
                        ownerDecimal7
                        ownerDecimal8
                        ownerDecimal9
                        ownerDecimal10
                        ownerDecimal11
                        ownerDecimal12
                        ownerDecimal13
                        ownerDecimal14
                        ownerDecimal15
                        ownerDecimal101
                        ownerDecimal102
                        ownerDecimal103
                        ownerDecimal104
                        ownerDecimal105
                        ownerDecimal106
                        ownerDecimal107
                        ownerDecimal108
                        ownerDecimal109
                        ownerDecimal110
                        ownerDecimal111
                        ownerDecimal112
                        ownerDecimal113
                        ownerDecimal114
                        ownerDecimal115
                        ownerDecimal116
                        ownerDecimal117
                        ownerDecimal118
                        ownerDecimal119
                        ownerDecimal120
                        ownerDecimal121
                        ownerDecimal122
                        ownerDecimal123
                        ownerDecimal124
                        ownerDecimal125
                        ownerDecimal126
                        ownerDecimal127
                        ownerDecimal128
                        ownerDecimal129
                        ownerDecimal130
                        ownerText101
                        ownerText102
                        ownerText103
                        ownerText104
                        ownerText105
                        ownerText106
                        ownerText107
                        ownerText108
                        ownerText109
                        ownerText110
                        ownerText111
                        ownerText112
                        ownerText113
                        ownerText114
                        ownerText115
                        ownerText116
                        ownerText117
                        ownerText118
                        ownerText119
                        ownerText120
                        ownerText121
                        ownerText122
                        ownerText123
                        ownerText124
                        ownerText125
                        ownerText126
                        ownerText127
                        ownerText128
                        ownerText129
                        ownerText130                        
                    }                                         
                    sovSortOrder

                }
                totalCount
            }
        }
        """;

    private readonly string stream_properties_with_filter_query = $$"""
        query {
            streamProperties(input: {
            streamID:"{0}",
            filters:{name: "effectiveFrom", operator: GREATER_OR_EQUAL, values: "{1}"},
            offset:0, 
            limit:{{PropertyHelpers.pageSize}}
            }) 
            {
                properties {
                    archipelagoID                                       
                    buildingDescription
                    fireProtectionDescription
                    securityDescription
                    city
                    contingency
                    country
                    county
                    geoCodeType
                    latitude
                    location
                    locationID
                    locationName
                    longitude
                    originalAddress
                    postalCode
                    propertyName
                    propertyStatus
                    region                        
                    state
                    streetAddress
                    customerProvidedGeocode
                    yearBuilt
                    yearLastUpgraded
                    yearsInPortfolio
        
                    baseFloodElevation
                    commodityHazardClass
                    contentsVulnerabilityWind
                    convectiveStormZone
                    exclusions
                    equipmentEarthquakeBracing                    
                    distanceToCoast
                    distanceToNearestFault
                    floodMissiles
                    floodZone
                    archipelagoFloodZone        
                    hailHazardClass
                    hazardSummary
                    landslideSusceptibility
                    lightningHazardClass
                    liquefactionSusceptibility
                    seismicDesignQuality
                    seismicDesignRValue
                    seismicHazardClass
                    seismicInspectionDate
                    seismicStatus
                    seismicZone
                    archipelagoSeismicZone
                    siteSoilClassification
                    sprinklerPipesSeismicallyBraced
                    tornadoHazardClass
                    tsunamiHazardClass
                    wildfireHazardClass
                    wildfireZone
                    windHazardClass
                    windMissiles
                    windZone
                    archipelagoWindZone
        
                    annualBaseRent
                    annualGrossProfit
                    annualServiceCharge  
                    
                    buildingReplacementCost
                    buildingReplacementCostDisplay
                    buildingReplacementCostPartner
                    buildingReplacementCostPercentDifference
                    buildingReplacementCostPercentDifferenceRange
                    buildingReplacementCostValueDifference, 
                    buildingValue
                    businessInterruptionRedundancy
                    businessInterruptionCostDisplay 
                    businessInterruptionPreparedness 
                    businessInterruptionCost
                    businessIncomeValue
                    contentsReplacementCost
                    contentsReplacementCostDisplay
                    contingentBusinessInterruptionFlag
                    contingentBusinessInterruptionComments
                    dependencyFlag
                    dependencyComments
                    dependencyCoveragePercentage
                    dependencyValue
                    effectiveFrom
                    electronicsValue
                    exchangeRate
                    exchangeRateDate
                    extraExpenseValue
                    fineArtJewelryValue
                    fireEstimatedMaximumLoss
                    fireProbableMaximumLoss
                    improvementsValue
                    inventoryValue
                    lastValuationDate
                    machineryValue
                    miscBusinessInterruptionValue                
                    miscBuildingReplacementCostValue
                    miscContentsValue
                    modelingAvailable
                    otherContentsValue
                    payrollValue
                    quakeScenarioEstimatedLoss
                    quakeScenarioUpperLoss
                    rentAndServiceIndemnityPeriod
                    replacementCostMethodology
                    replacementCostPerFloorAreaPartner
                    replacementCostPerFloorAreaPartnerDifference
                    replacementCostPerSquareFootage
                    replacementCostPerSquareFootageDisplay
                    stockThroughputFactor
                    stockThroughputInventoryValue
                    stockThroughputExcessInventoryValue
                    totalInsuredValue
                    totalInsuredValueDisplay
                    valuationCurrency
        
                    annualEstimatedRent
                    annualLossOfRevenue                  
                    acquiredOrBuilt
                    acquisitionOrOccupancyDate
                    businessContinuityPlans
                    controlOfCombustiblesProgram
                    dispositionDate                 
                    fireProtectionInspectionProgram
                    fireRatingForRoofCovering
                    fireRatingForWallSiding
                    firewiseCommunityParticipation
                    flammableLiquidGasLineManagement
                    flammableLiquidDescription
                    flammableLiquidStorageLocation
                    flammableLiquidCapacity
                    flammableLiquidsOnsite
                    flammableLiquidProgram
                    generatorTestingProgram
                    hasSeismicInspectionReport
                    hotWorkProgram
                    lastEngineeringVisitDate
                    leasedOrOwned
                    lossEngineeringReportPresent
                    multiTenant
                    occupancyDescription
                    occupancyType
                    occupancyTypeATC
                    occupancyTypeNAICS
                    occupancyTypeSIC
                    orgName
                    organizationLevelOne
                    organizationLevelTwo
                    organizationLevelThree
                    organizationLevelFour
                    owner                   
                    portableFireExtinguisherProgram
                    portfolio
                    primaryTenantNAICS
                    propertyManager
                    roofInspectionProgram
                    smokingControlsProgram
                    specificOccupancy
                    valueInspectionProgram
                    yearBuilt
                    yearsInPortfolio
                 
                    accessoryStructures
                    backupGenerator
                    backupGeneratorDieselStorageCapacity
                    backupGeneratorLocation                           
                    basementFinishType
                    buildingCondition
                    buildingExteriorOpening
                    buildingFootprintClass
                    buildingFoundationType
                    buildingRedundancy
                    chargingStationPower
                    chimneys
                    constructionDescription
                    constructionQuality
                    constructionType
                    constructionTypeAIR
                    constructionTypeATC
                    constructionTypeArchipelago
                    constructionTypeCluster
                    constructionTypeISO
                    constructionTypeRMS                 
                    crippleWalls
                    deck
                    defensibleSpace
                    distanceToNearestBuilding
                    doorAndFrameType
                    elevationOfGroundOrBasementLevelMEPEquipment                  
                    eqSpecialSystems
                    equipmentEarthquakeBracing
                    exteriorFuelStorage
                    fencesWithinFiveFeet
                    fireDetectionSystems
                    firePumpFlowRate
                    firePumpPowerSupply
                    firePumpBackupPeriod
                    firePumpTestResults
                    firePumpChurnRate
                    firePumpExcessCapacity
                    firstFloorElevation
                    firstFloorHeight                    
                    floorArea
                    floorOfInterest                   
                    floorSystem
                    foundationToFrameConnection
                    glassPercentage
                    glassType
                    heightOfRackStorage
                    highHazardAreaSprinklerType
                    hydrantFlowTestResultsStatic
                    hydrantFlowTestResultsResidual
                    inRackSprinklerType
                    interiorPartitions                    
                    naturalGasAutomaticShutoff
                    naturalGasPipesSeismicallyBraced
                    numberOfBuildings
                    numberOfFireHazardAreas
                    numberOfHighPriorityRecommendations
                    numberOfStoriesAboveGround
                    numberOfStoriesBelowGround
                    numberOfUnits
                    ornamentation
                    rackStoragePresent
                    residentialGarage
                    roofChimneysAnchorage
                    roofCopingAndFlashing
                    roofDeckAnchorage
                    roofDeckType
                    roofDescription
                    roofDrainageType
                    roofEquipmentAnchorage
                    roofEstimatedReplacementYear
                    roofGeometry
                    roofHailMitigation
                    roofImage
                    roofInstallYear
                    roofParapets
                    roofOverhang
                    roofScreensSignsAnchorageandBracing
                    roofSolarPanelCoverage
                    roofSolarPanelDescription
                    roofSolarPanelOwnership
                    roofStructures
                    roofSystem
                    roofSystemAnchorage
                    roofVent
                    roofingMaterialAnchorage
                    rooftopSolarPanels
                    rooftopWaterTanks
                    seismicDesignQuality
                    seismicDesignRValue
                    seismicStatus
                    shortColumnConcrete
                    sprinklerHeadSize
                    sprinklerHeadTemperatureRating
                    soffit
                    softStory
                    solarPanelPower
                    storageArrangementDescription
                    structuralDescription
                    structuralSystemUpgraded
                    structuralUpgradeType
                    surfaceRoughnessWind
                    torsion
                    treeExposure
                    verticalIrregularity
                    wallCladdingSystem
                    waterHeaterBracing
                    waterSupplyFlowRate
                    waterSupplyTime
                    windowProtection
                    windowType
                    yearLastUpgraded
                    
        
        
                    buildingSprinklered
                    buildingSprinklerType
                    contentsVulnerabilityFlood
                    contentsVulnerabilityVerticalDistributionOfContents
                    contentsVulnerabilityWind
                    emergencyFloodProtectionMeasures
                    percentageSprinklered
                    permanentFloodMeasuresPresent
                    remoteMonitoringOfSprinklerSystem
                    sprinklerLeakDetectionSystem
                    suitabilityOfFireProtectionMeasures
        
                    attributeProvenance {                    
                        sources                        
                        externalSourceURLs
                    }
                    comment
                    enriched   
                    ownerAttributes {
                        ownerText1
                        ownerText2
                        ownerText3
                        ownerText4
                        ownerText5
                        ownerText6
                        ownerText7
                        ownerText8
                        ownerText9
                        ownerText10
                        ownerText11
                        ownerText12
                        ownerText13
                        ownerText14
                        ownerText15
                        ownerDecimal1
                        ownerDecimal2
                        ownerDecimal3
                        ownerDecimal4
                        ownerDecimal5
                        ownerDecimal6
                        ownerDecimal7
                        ownerDecimal8
                        ownerDecimal9
                        ownerDecimal10
                        ownerDecimal11
                        ownerDecimal12
                        ownerDecimal13
                        ownerDecimal14
                        ownerDecimal15
                        ownerDecimal101
                        ownerDecimal102
                        ownerDecimal103
                        ownerDecimal104
                        ownerDecimal105
                        ownerDecimal106
                        ownerDecimal107
                        ownerDecimal108
                        ownerDecimal109
                        ownerDecimal110
                        ownerDecimal111
                        ownerDecimal112
                        ownerDecimal113
                        ownerDecimal114
                        ownerDecimal115
                        ownerDecimal116
                        ownerDecimal117
                        ownerDecimal118
                        ownerDecimal119
                        ownerDecimal120
                        ownerDecimal121
                        ownerDecimal122
                        ownerDecimal123
                        ownerDecimal124
                        ownerDecimal125
                        ownerDecimal126
                        ownerDecimal127
                        ownerDecimal128
                        ownerDecimal129
                        ownerDecimal130
                        ownerText101
                        ownerText102
                        ownerText103
                        ownerText104
                        ownerText105
                        ownerText106
                        ownerText107
                        ownerText108
                        ownerText109
                        ownerText110
                        ownerText111
                        ownerText112
                        ownerText113
                        ownerText114
                        ownerText115
                        ownerText116
                        ownerText117
                        ownerText118
                        ownerText119
                        ownerText120
                        ownerText121
                        ownerText122
                        ownerText123
                        ownerText124
                        ownerText125
                        ownerText126
                        ownerText127
                        ownerText128
                        ownerText129
                        ownerText130                        
                    }                                         
                    sovSortOrder
                }
                totalCount
            }
        }
        """;
   
    private readonly string property_losses_query = $$"""
           query {
                propertyLosses(input:{streamID:"{0}",archipelagoPropertyID:"{1}"})
                {
                    losses {
                        propertyClientID
                        archipelagoLossID
                        archipelagoPropertyID
                        catastrophe
                        catastropheEventName
                        clientClaimID
                        deductible
                        generalCauseOfLoss
                        grossLossIncurred
                        grossTotalLoss
                        lossCurrency
                        lossDate
                        lossDescription
                        lossStatus
                        occurrenceNumber
                        orgName
                        paid
                        reserve
                        specificCauseOfLoss
                     }
                     totalCount
                }
        }
        """;

    public async Task<PropertyResponse> GetStreamProperties(string streamId, string? effectiveFrom)
    {

        var results = new PropertyResponse { streamId = streamId, properties = [], totalCount = 0 };

        // Request API token        
        var accessToken = PropertyHelpers.authManager != null
            ? await PropertyHelpers.authManager.GetAccessTokenAsync()
            : string.Empty;

        var new_stream_properties_query = string.IsNullOrEmpty(effectiveFrom) ? stream_properties_query : stream_properties_with_filter_query;
        new_stream_properties_query = new_stream_properties_query.Replace("{0}", $"{streamId}");

        if (string.IsNullOrEmpty(effectiveFrom) == false)
        {
            new_stream_properties_query = new_stream_properties_query.Replace("{1}", $"{effectiveFrom}");
        }

        var stringResult = await Utils.RunQuery(new_stream_properties_query, accessToken, PropertyHelpers.api_url);

        //PrintProperties(stringResult);

        // traverse streamsPage response
        var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);
        if (jsonResult == null || jsonResult.ContainsKey("data") == false || jsonResult["data"] == null)
        {
            return results;
        }
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult["data"].ToString() ?? "");
        if (jsonData == null)
        {
            return results;
        }
        var jsonStreamsPage = JsonSerializer.Deserialize<Dictionary<string, object>>((jsonData["streamProperties"] ?? "").ToString() ?? "");

        if (jsonStreamsPage == null)
        {
            return results;
        }

        var jsonProperties = JsonSerializer.Deserialize<List<object>>((jsonStreamsPage["properties"] ?? "").ToString() ?? "");
        var jsonPageInfo = JsonSerializer.Deserialize<int>(jsonStreamsPage["totalCount"].ToString() ?? "");

        results.properties = jsonProperties;
        results.totalCount = jsonPageInfo;

        return results;
    }

    public async Task<List<object>> GetPropertiesPagination(string streamId, int totalCount, string? effectiveFrom)
    {      
        // traverse initial streamsPage response
        if (totalCount == 0) return [];


        // Continue to run streamsPage with next cursor while hasNextPage=true
        int currPage = 2;
        var results = new List<object>();
        var done = false;
        var prevOffset = 0;
        var adjustedOffset = PropertyHelpers.pageSize;

        var new_stream_properties_query = string.IsNullOrEmpty(effectiveFrom) ? stream_properties_query : stream_properties_with_filter_query;

        new_stream_properties_query = new_stream_properties_query.Replace("{0}", $"{streamId}");

        if (string.IsNullOrEmpty(effectiveFrom) == false)
        {
            new_stream_properties_query = new_stream_properties_query.Replace("{1}", $"{effectiveFrom}");
        }

        while (done == false)
        {
            // Request API token        
            var accessToken = PropertyHelpers.authManager != null
                ? await PropertyHelpers.authManager.GetAccessTokenAsync()
                : string.Empty;

            adjustedOffset = prevOffset > 0 ? adjustedOffset + PropertyHelpers.pageSize : adjustedOffset;

            // pass offset and stream id to Stream Properties query

            new_stream_properties_query = new_stream_properties_query.Replace($"offset:{prevOffset}", $"offset:{adjustedOffset}");

            // traverse next streamsPage response for next cursor / page
            var stringResult = await Utils.RunQuery(new_stream_properties_query, accessToken, PropertyHelpers.api_url);
            //PrintProperties(stringResult);
            var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);
            if (jsonResult == null || jsonResult.ContainsKey("data") == false || jsonResult["data"] == null) { 
                continue; 
            }
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult["data"].ToString() ?? "");
            if (jsonData == null) continue;
            var jsonStreamProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData["streamProperties"].ToString() ?? "");
            if (jsonStreamProperties == null) continue;
            var streamProperties = JsonSerializer.Deserialize<List<object>>(jsonStreamProperties["properties"].ToString() ?? "");

            ++currPage;

            if (streamProperties != null)
            {
                results.AddRange(streamProperties);
            }


            var retreivedCnt = results.Count;
            done = retreivedCnt <= 0 || streamProperties == null || streamProperties.Count <= 0 || retreivedCnt + PropertyHelpers.pageSize >= totalCount;

            if (!done)
            {
                prevOffset = adjustedOffset;
            }
        }
        return results;
    }

    public async Task<List<Dictionary<string, object>>> GetPropertyLosses(string streamId, List<object> properties)
    {
        var numberOfProperties = properties.Count;
        var propertyPartitions = numberOfProperties > 1000 ? properties.Partition(numberOfProperties / 1000) : [properties];
        List<Task> TaskList = [];
        List<Dictionary<string, object>> dataPropertyLosses = [];

        foreach (var partition in propertyPartitions)
        {
            TaskList.Add(Task.Run(async () => {              
                var propertyLosses = await LoadPropertyLosses(streamId, partition);
                if (propertyLosses != null && propertyLosses.Count > 0)
                {
                    dataPropertyLosses.AddRange(propertyLosses);
                }
            }));
        }

        await Task.WhenAll(TaskList);

        return dataPropertyLosses;
    }

    public async Task<List<Dictionary<string, object>>> LoadPropertyLosses(string streamId, List<object> properties)
    {
        List<Dictionary<string, object>> dataPropertyLosses = [];

        foreach (var property in properties)
        {
            // Request API token        
            var accessToken = PropertyHelpers.authManager != null
                ? await PropertyHelpers.authManager.GetAccessTokenAsync()
                : string.Empty;

            var jsonProperty = JsonSerializer.Deserialize<Dictionary<string, object>>(property.ToString() ?? "");

            if (jsonProperty == null) { continue; }

            var archipelagoID = jsonProperty["archipelagoID"].ToString() ?? "";

            if (string.IsNullOrWhiteSpace(archipelagoID)) { continue; }
            // request API token           
            //IConfiguration configuration = Utils.ReadSettings("appsettings.json");
            //var accessToken = await Utils.GetToken(configuration, environment);

            var new_property_losses_query = property_losses_query.Replace("{0}", $"{streamId}");
            new_property_losses_query = new_property_losses_query.Replace("{1}", $"{archipelagoID}");
            var stringResult = await Utils.RunQuery(new_property_losses_query, accessToken, PropertyHelpers.api_url);

            var jsonResult = JsonSerializer.Deserialize<Dictionary<string, object>>(stringResult);
            if (jsonResult == null || jsonResult.ContainsKey("data")==false || jsonResult["data"] == null)
            {
                continue;
            }

            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResult["data"].ToString() ?? "");

            if (jsonData != null)
            {
                var jsonPropertyLosses = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData["propertyLosses"].ToString() ?? "");

                if (jsonPropertyLosses != null)
                {
                    var losses = JsonSerializer.Deserialize<List<object>>(jsonPropertyLosses["losses"].ToString() ?? "");
                    if (losses == null || losses.Count <= 0) continue;
                }
            }

            var propertyLosses = jsonData ?? [];
            if (propertyLosses != null && propertyLosses.Count > 0) { propertyLosses.Add("archipelagoID", archipelagoID); dataPropertyLosses.Add(propertyLosses); }
        }

        return dataPropertyLosses;

    }     
}