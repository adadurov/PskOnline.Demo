﻿namespace PskOnline.Server.Service.ViewModels
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using Newtonsoft.Json.Linq;

  public class TestPostDto
  {
    public string Id { get; set; }

    [Required(ErrorMessage = "InspectionId is required")]
    public string InspectionId { get; set; }

    [Required(ErrorMessage = "MethodId is required")]
    public string MethodId { get; set; }

    public string MethodVersion { get; set; }

    public DateTimeOffset StartTimeUtc { get; set; }

    public DateTimeOffset FinishTimeUtc { get; set; }

    /// <summary>
    /// Raw test data recorded by the method client plugin
    /// (in a Web app, a mobile app or in a desktop app)
    /// </summary>
    public JObject MethodRawDataJson { get; set; }

    /// <summary>
    /// Processed test data generated by the
    /// method data processing plugin (on the server)
    /// </summary>
    public JObject MethodProcessedDataJson { get; set; }

    public string Comment { get; set; }
  }
}
