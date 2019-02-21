using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "False sense of security and cumbersome")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "SupportFunctions.Model",
        Justification = "false positive")]
[assembly:
    SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Conflicts with CA2000", Scope = "member",
        Target = "~M:SupportFunctions.Model.TimeSeriesChart.InitChart(System.Drawing.Size)")]
[assembly:
    SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Conflicts with CA2000", Scope = "member",
        Target = "~M:SupportFunctions.Model.TimeSeriesChart.InitChartArea")]
// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File". You do not need to add suppressions to this file manually.