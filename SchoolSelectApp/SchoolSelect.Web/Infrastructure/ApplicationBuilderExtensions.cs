using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Migrations;

namespace SchoolSelect.Web.Infrastructure
{
    /// <summary>
    /// Разширения за IApplicationBuilder за миграция и инициализация на данни
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Мигрира старите формули към новия структуриран формат
        /// </summary>
        /// <param name="app">Инстанция на IApplicationBuilder</param>
        /// <returns>Същата инстанция на IApplicationBuilder</returns>
        public static IApplicationBuilder MigrateFormulaData(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Използваме съществуващия клас FormulaMigration
                FormulaMigration.MigrateFormulasAsync(unitOfWork).Wait();
            }

            return app;
        }
    }
}