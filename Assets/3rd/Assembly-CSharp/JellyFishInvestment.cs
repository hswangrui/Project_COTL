using System;
using System.Collections.Generic;

[Serializable]
public class JellyFishInvestment
{
	public int InitialInvestment;

	public int ActualInvestedAmount;

	public int NewInvestment;

	public int InvestmentDay;

	public List<JellyFishInvestmentDay> InvestmentDays = new List<JellyFishInvestmentDay>();

	public int LastDayCheckedInvestment = -1;
}
