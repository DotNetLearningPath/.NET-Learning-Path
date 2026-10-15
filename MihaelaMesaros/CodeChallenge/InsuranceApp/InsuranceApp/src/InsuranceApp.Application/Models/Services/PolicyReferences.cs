using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Models.Services;

public sealed record PolicyReferences(
    Client Client,
    Building Building,
    Broker Broker,
    Currency Currency);
