[<AutoOpen>]
module Farmer.Resources.ContainerRegistry

open Farmer
open Farmer.Models

/// Container Registry SKU
type ContainerRegistrySku =
    | Basic
    | Standard
    | Premium

// TODO: networkRuleSet
// TODO: policies
// TODO: encryption
// TODO: dataEndpointEnabled

type ContainerRegistryConfig =
    {
      Name : ResourceName
      Sku : ContainerRegistrySku
      AdminUserEnabled : bool }
    
type ContainerRegistryBuilder() =
    member _.Yield _ =
        { Name = ResourceName.Empty
          Sku = Basic
          AdminUserEnabled = false }
    [<CustomOperation "name">]
    member _.Name (state:ContainerRegistryConfig, name) = { state with Name = ResourceName name }
    [<CustomOperation "sku">]
    member _.Sku (state:ContainerRegistryConfig, sku) = { state with Sku = sku }
    [<CustomOperation "enable_admin_user">]
    member _.EnableAdminUser (state:ContainerRegistryConfig) = { state with AdminUserEnabled = true }

module Converters =
    let containerRegistry location (config:ContainerRegistryConfig) : ContainerRegistry =
        { Name = config.Name
          Location = location
          Sku = config.Sku.ToString().Replace("_", ".")
          AdminUserEnabled = config.AdminUserEnabled }

    module Outputters =
        let containerRegistry (service:Farmer.Models.ContainerRegistry) =
            {| name = service.Name.Value
               ``type`` = "Microsoft.ContainerRegistry/registries"
               apiVersion = "2019-05-01"
               sku = {| name = service.Sku |}
               location = service.Location.Value
               tags = {||}
               properties = {|
                              adminUserEnabled = service.AdminUserEnabled |} |}

let containerRegistry = ContainerRegistryBuilder()

type Farmer.ArmBuilder.ArmBuilder with
    member this.AddResource(state:ArmConfig, config) =
        { state with
            Resources = ContainerRegistry (Converters.containerRegistry state.Location config) :: state.Resources
        }
    member this.AddResources (state, configs) = addResources<ContainerRegistryConfig> this.AddResource state configs
