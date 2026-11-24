global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Market.Domain.Entities.Identity;
// Polica (#6): code list entities (Manufacturer, DosageForm, WriteOffReason) for DbSets, configurations and seeders
global using Market.Domain.Entities.CodeLists;
// Polica (#7): PharmacyEntity for its DbSet, configuration and the demo seeder (City is in CodeLists above)
global using Market.Domain.Entities.Pharmacies;
global using Microsoft.AspNetCore.Identity;