namespace RaynMaker.Portfolio.Entities

open System

type StockBought = {
    Date : DateTime
    Isin : string
    Name : string
    Count : float
    Price : float
    Fee : float
    }

type StockSold = {
    Date : DateTime
    Isin : string
    Name : string
    Count : float
    Price : float
    Fee : float
    }

type DividendReceived = {
    Date : DateTime
    Isin : string
    Name : string
    Price : float
    Fee : float
    }

type DepositAccounted = {
    Date : DateTime
    Value : float
    }

type SavingsPlanRateAccounted = {
    Date : DateTime
    Value : float
    }

type DisbursementAccounted = {
    Date : DateTime
    Value : float
    }

type InterestReceived = {
    Date : DateTime
    Value : float
    }

type Event = 
    | StockBought 
    | StockSold 
    | DividendReceived 
    | DepositAccounted 
    | SavingsPlanRateAccounted 
    | DisbursementAccounted 
    | InterestReceived 



