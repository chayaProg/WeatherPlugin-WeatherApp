using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WeatherPlugin
{
    public static class WeatherLogic
    {

        /// <summary>
        /// Convert temperature - Fahrenheit <-> Celsius
        /// </summary>
        /// <param name="entity">The entity with one temperature value</param>
        public static void ConvertTemperatures(Entity entity, IOrganizationService service)
        {

            if (entity.Contains(LogicalNames.TemperatureC) && entity[LogicalNames.TemperatureC] != null)
            {
                decimal celsius = (decimal)entity[LogicalNames.TemperatureC];

                //Formula for converting Celsius to Fahrenheit
                decimal fahrenheit = (celsius * 9 / 5) + 32;
                entity[LogicalNames.TemperatureF] = fahrenheit;
            }
            else if (entity.Contains(LogicalNames.TemperatureF) && entity[LogicalNames.TemperatureF] != null)
            {
                decimal fahrenheit = (decimal)entity[LogicalNames.TemperatureF];

                //Formula for converting Fahrenheit to Celsius  
                decimal celsius = (fahrenheit - 32) * 5 / 9;
                entity[LogicalNames.TemperatureC] = celsius;
            }
            /*service.Update(entity);*/
        }


        /// <summary>
        /// Sets the status reason based on temperature comparison between current and previous entity.
        /// </summary>
        public static int  SetStatusReason(Entity current, Entity previous, IOrganizationService service)
        {
            if (!current.Contains(LogicalNames.TemperatureC)) return 0;

            decimal currentTemp = current.GetAttributeValue<decimal>(LogicalNames.TemperatureC);
            decimal previousTemp = previous.GetAttributeValue<decimal>(LogicalNames.TemperatureC);

            OptionSetValue statusReason;
            if (currentTemp > previousTemp)
                statusReason = new OptionSetValue(StatusReasonValue.Warmer);
            else if (currentTemp < previousTemp)
                statusReason = new OptionSetValue(StatusReasonValue.Colder);
            else
                statusReason = new OptionSetValue(StatusReasonValue.NoChange);

            current[LogicalNames.StatusReason] = statusReason;

            service.Update(current);
            int currentStatusReason = statusReason.Value;
            return currentStatusReason;
        }


        /// <summary>
        /// Updates the change counter based on temperature and status reason changes
        /// </summary>
        /// <exception cref="InvalidPluginExecutionException">Thrown if both temperature and status are the same as previous</exception>
        /// <param name="currentStatusReason">get the current status reason based on previous function that updated it</param>
        public static void HandleChangeCounter(Entity current, Entity previous, IOrganizationService service, int currentStatusReason)
        {
            OptionSetValue previousStatusOption = previous.GetAttributeValue<OptionSetValue>(LogicalNames.StatusReason);

            if (previousStatusOption == null)
                return;

            int previousStatus = previousStatusOption.Value;
            int counter = previous.GetAttributeValue<int?>(LogicalNames.ChangeCounter) ?? 0;
            
            if (currentStatusReason != previousStatus&& currentStatusReason!= StatusReasonValue.NoChange)
            {
                counter++;
                if (counter >10)
                    counter = 0;
            }

            current[LogicalNames.ChangeCounter] = counter;

        /*    service.Update(current);*/
        }


        /// <summary>
        /// change all the previous records except current - IsActive=true?->IsActive=false
        /// </summary>
        public static void DeactivatePreviousEntities(Entity current, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression(LogicalNames.TableName)
            {
                ColumnSet = new ColumnSet(LogicalNames.IsActive, LogicalNames.Id),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(LogicalNames.IsActive, ConditionOperator.Equal, true),
                        new ConditionExpression(LogicalNames.Id, ConditionOperator.NotEqual, current.Id)
                    }
                }
            };

            EntityCollection results = service.RetrieveMultiple(query);
            foreach (var report in results.Entities)
            {
                report[LogicalNames.IsActive] = false;
                service.Update(report);
            }
            current[LogicalNames.IsActive] = true;

         /*   service.Update(current);*/
        }
    }
}

