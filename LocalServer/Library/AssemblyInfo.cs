using System.Runtime.CompilerServices;

//!!!
//[assembly: InternalsVisibleTo("Test, PublicKey=" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.LocalForwarder.Test")]

internal static class AssemblyInfo
{
    public const string PublicKey = "0024000004800000940000000602000000240000525341310004000001000100319b35b21a993df850846602dae9e86d8fbb0528a0ad488ecd6414db798996534381825f94f90d8b16b72a51c4e7e07cf66ff3293c1046c045fafc354cfcc15fc177c748111e4a8c5a34d3940e7f3789dd58a928add6160d6f9cc219680253dcea88a034e7d472de51d4989c7783e19343839273e0e63a43b99ab338149dd59f";
}