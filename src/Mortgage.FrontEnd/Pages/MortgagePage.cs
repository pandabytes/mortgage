using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Mortgage.FrontEnd.Pages;

public partial class MortgagePage : ComponentBase
{
  private record Amortization(
    DateOnly Date,
    float StartBalance,
    int LoanTerm,
    float Interest,
    float MonthlyPayment,
    float PrincipalPortion,
    float InterestPortion,
    float RemainingBalance,
    float TotalInterest
  );

  private bool _isLoading = false;

  private static readonly IEnumerable<int> PageSizeOptions = new int[] { 5, 10, 20, 50, 100 };

  private const int InitialPageSize = 10;

  private IList<Amortization> _amortizationList = new List<Amortization>();

  private IList<Amortization> _selectedAmortization = new List<Amortization>();

  private string _date = "12/2023";

  private float _startBalance = 127685.00f;

  private int _loanTerm = 10;

  private float _interestRate = 0.07f;

  private float _monthlyPayment = 1482.53f;

  private void OnSubmit()
  {
    _amortizationList.Clear();
    var dateTime = DateTime.ParseExact(_date, "MM/yyyy", CultureInfo.InvariantCulture);
    var dateOnly = DateOnly.FromDateTime(dateTime);

    var firstInterestPortion = CalculateInterestPortions(_startBalance, _interestRate, _monthlyPayment);
    var firstPrincipalPortion = CalculatePrincipalPortions(firstInterestPortion, _monthlyPayment);
    var firstAmortization = new Amortization(
      Date: dateOnly,
      StartBalance: _startBalance,
      LoanTerm: _loanTerm,
      Interest: _interestRate,
      MonthlyPayment: _monthlyPayment,
      PrincipalPortion: firstPrincipalPortion,
      InterestPortion: firstInterestPortion,
      RemainingBalance: _startBalance - firstPrincipalPortion,
      TotalInterest: firstInterestPortion
    );

    var amortizationList = new List<Amortization>() {firstAmortization};
    var numberOfMonths = _loanTerm * 12;
    for (int i = 1; i < numberOfMonths; i++)
    {
      var prevAmortization = amortizationList[i-1];

      var startBalance = prevAmortization.RemainingBalance;
      var interest = prevAmortization.Interest;
      var totalInterest = prevAmortization.TotalInterest;
      var monthlyPayment = prevAmortization.MonthlyPayment;

      var interestPortion = CalculateInterestPortions(startBalance, interest, monthlyPayment);
      var principalPortion = CalculatePrincipalPortions(interestPortion, monthlyPayment);

      var amortization = new Amortization(
        Date: prevAmortization.Date.AddMonths(1),
        StartBalance: startBalance,
        LoanTerm: prevAmortization.LoanTerm,
        Interest: prevAmortization.Interest,
        MonthlyPayment: prevAmortization.MonthlyPayment,
        PrincipalPortion: principalPortion,
        InterestPortion: interestPortion,
        RemainingBalance: startBalance - principalPortion,
        TotalInterest: totalInterest + interestPortion
      );

      amortizationList.Add(amortization);

      // If true it means we have paid off the mortgage!
      if (amortization.RemainingBalance <= 0)
      {
        break;
      }
    }

    _amortizationList = amortizationList;
  }

  private void OnClear()
  {
    _amortizationList.Clear();
  }

  private static float CalculateInterestPortions(float startBalance, float interest, float mortgagePayment)
    => mortgagePayment - (mortgagePayment - (interest / 12 * startBalance));

  private static float CalculatePrincipalPortions(float interestPortion, float mortgagePayment)
    => mortgagePayment - interestPortion;

  private static string ConvertToCurrencyFormat(float value)
    => value.ToString("C", CultureInfo.CurrentCulture);

  private static string ConvertToPercentageFormat(float value)
    => string.Format("{0:P2}", value);
}
