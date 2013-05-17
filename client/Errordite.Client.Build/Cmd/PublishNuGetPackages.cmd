cd "%1/Errordite.Client"
nuget push Errordite.Client.%2.nupkg
cd "%1/Errordite.Client.Mvc2"
nuget push Errordite.Client.Mvc2.%2.nupkg
cd "%1/Errordite.Client.Mvc3"
nuget push Errordite.Client.Mvc3.%2.nupkg
cd "%1/Errordite.Client.Mvc4"
nuget push Errordite.Client.Mvc4.%2.nupkg
cd "%1/Errordite.Client.Log4net"
nuget push Errordite.Client.Log4net.%2.nupkg