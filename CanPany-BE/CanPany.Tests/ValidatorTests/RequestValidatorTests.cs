using CanPany.Application.Validators;
using CanPany.Application.DTOs.Auth;
using CanPany.Application.DTOs.Jobs;
using CanPany.Application.DTOs.Applications;
using CanPany.Application.DTOs;
using CanPany.Application.DTOs.Alerts;
using FluentValidation.TestHelper;
using Xunit;

namespace CanPany.Tests.ValidatorTests;

public class RequestValidatorTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly CreateJobRequestValidator _createJobValidator = new();
    private readonly CreateApplicationRequestValidator _createApplicationValidator = new();
    private readonly CreateReportRequestValidator _createReportValidator = new();
    private readonly CreateJobAlertRequestValidator _jobAlertValidator = new();
    private readonly CreateCandidateAlertRequestValidator _candidateAlertValidator = new();

    #region RegisterRequest Tests
    [Fact]
    public void RegisterRequest_Valid_Passes()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Role = "Candidate"
        };
        var result = _registerValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterRequest_PasswordMismatch_Fails()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "Password123",
            ConfirmPassword = "DifferentPassword",
            Role = "Candidate"
        };
        var result = _registerValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void RegisterRequest_WeakPassword_Fails()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "simple",
            ConfirmPassword = "simple",
            Role = "Candidate"
        };
        var result = _registerValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    #endregion

    #region CreateJobRequest Tests
    [Fact]
    public void CreateJobRequest_Valid_Passes()
    {
        var request = new CreateJobRequest(
            CompanyId: "compId",
            Title: "Software Engineer",
            Description: "We are looking for a senior software engineer with 10 years of experience.",
            SkillIds: new List<string> { "csharp", "dotnet" },
            Deadline: DateTime.UtcNow.AddDays(7)
        );
        var result = _createJobValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateJobRequest_ShortDescription_Fails()
    {
        var request = new CreateJobRequest(
            CompanyId: "compId",
            Title: "Software Engineer",
            Description: "Too short",
            SkillIds: new List<string> { "csharp" }
        );
        var result = _createJobValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
    #endregion

    #region CreateApplicationRequest Tests
    [Fact]
    public void CreateApplicationRequest_Valid_Passes()
    {
        var request = new CreateApplicationRequest(
            JobId: "jobId",
            CVId: "cvId",
            CoverLetter: "I am interested in this position.",
            ExpectedSalary: 5000
        );
        var result = _createApplicationValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
    #endregion

    #region CreateReportRequest Tests
    [Fact]
    public void CreateReportRequest_Valid_Passes()
    {
        var request = new CreateReportDto(
            ReportedUserId: "userId",
            Reason: "Harassment",
            Description: "The user was sending offensive messages."
        );
        var result = _createReportValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
    #endregion

    #region AlertRequest Tests
    [Fact]
    public void CreateJobAlertRequest_Valid_Passes()
    {
        var request = new CreateJobAlertRequest(
            Name: "My Tech Alert",
            Location: "San Francisco",
            MinBudget: 5000,
            Frequency: "Daily"
        );
        var result = _jobAlertValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateJobAlertRequest_InvalidFrequency_Fails()
    {
        var request = new CreateJobAlertRequest(
            Name: "Alert",
            Location: "Online",
            Frequency: "Hourly"
        );
        var result = _jobAlertValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Frequency);
    }

    [Fact]
    public void CreateCandidateAlertRequest_MaxExpLessThanMinExp_Fails()
    {
        var request = new CreateCandidateAlertRequest(
            Name: "Senior Alert",
            Location: "Local",
            MinExperience: 5,
            MaxExperience: 3
        );
        var result = _candidateAlertValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MaxExperience);
    }
    #endregion
}
