

using System;
using System.IdentityModel.Protocols.WSTrust;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace WeatherPlugin
{
    public class WeatherPlugin : IPlugin
    {
        /// <summary>
        /// Main entry point for the plugin execution.
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            
            // Get context of the current plugin execution (who triggered it, on what, etc.)
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Create service to interact with Dataverse (retrieve/update.. entities)
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)
                serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            /* Check that the plugin was triggered with a target entity (created)
               Target - the standard key used by CRM to represent the Entity being acted on-
              this is the actual record that triggered the plugin execution ( a newly created)
            */
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity entity &&
                entity.LogicalName == LogicalNames.TableName)
            {
                // Convert temperature between Celsius and Fahrenheit if needed
                WeatherLogic.ConvertTemperatures(entity,service);

                // Retrieve the most recent previous record (not including the current one)
                QueryExpression query = new QueryExpression(LogicalNames.TableName)
                {
                    // createdon- a system field that stores the date and time the record was created
                    ColumnSet = new ColumnSet(LogicalNames.TemperatureC, LogicalNames.StatusReason, LogicalNames.ChangeCounter ,"createdon"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression(LogicalNames.Id, ConditionOperator.NotEqual, entity.Id)
                        }
                    },
                    Orders =
                    {
                        new OrderExpression("createdon", OrderType.Descending)
                    },
                    TopCount = 1
                };
                EntityCollection results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    // First-ever record default values
                    entity[LogicalNames.ChangeCounter] = 0;
                    entity[LogicalNames.StatusReason] = new OptionSetValue(StatusReasonValue.NoChange);
                    entity[LogicalNames.IsActive] = true;

                }
                else
                {
                    var previous = results.Entities[0];

                    // Set status reason based on temperature comparison
                    //returnt the current status reason to changeCounter function
                    int currentStatusReason = WeatherLogic.SetStatusReason(entity, previous,service);
 
                    // Update change counter based on difference from previous
                    WeatherLogic.HandleChangeCounter(entity, previous,service, currentStatusReason);

                }

                //  Deactivate all other records, leave only current active
                WeatherLogic.DeactivatePreviousEntities(entity, service);

                service.Update(entity);
            }


        }
    }
}
