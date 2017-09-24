namespace RaynMaker.Portfolio.Entities

open System

type StockBought = {
    Date : DateTime
    Isin : string
    Name : string
    Count : int
    Price : float
    Fee : float
    }

type StockSold = {
    Date : DateTime
    Isin : string
    Name : string
    Count : int
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

type DomainEvent = 
    | StockBought of StockBought
    | StockSold of StockSold
    | DividendReceived of DividendReceived
    | DepositAccounted of DepositAccounted
    | SavingsPlanRateAccounted of SavingsPlanRateAccounted
    | DisbursementAccounted of DisbursementAccounted
    | InterestReceived of InterestReceived



