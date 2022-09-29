job(".NET Core GPNA_WwIoServerConnector build,test and publish"){
    container(image = "mcr.microsoft.com/dotnet/sdk:5.0"){
        env["FEED_URL"] = "https://nuget.pkg.jetbrains.space/gpna/p/gpna-common/common/v3/index.json"
        shellScript {
            content = """
                echo Run build...
                dotnet build  
                echo Run tests...
                dotnet test ./GPNA.GPNA_WwIoServerConnector.Tests/
                echo Publish NuGet package...
                /*chmod +x publish.sh*/
                /*./publish.sh*/
            """
        }
    }
}
