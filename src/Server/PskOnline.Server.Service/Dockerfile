FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src/Server/PskOnline.Server.Service/PskOnline.Server.Service.csproj src/Server/PskOnline.Server.Service/
COPY src/Server/PskOnline.Server.ObjectModel/PskOnline.Server.ObjectModel.csproj src/Server/PskOnline.Server.ObjectModel/
COPY src/Server/Plugins/RusHydro/PskOnline.Plugins.RusHydro.Web/PskOnline.Server.Plugins.RusHydro.Web.csproj src/Server/Plugins/RusHydro/PskOnline.Plugins.RusHydro.Web/
COPY src/Server/PskOnline.Server.Shared/PskOnline.Server.Shared.csproj src/Server/PskOnline.Server.Shared/
COPY src/Server/Plugins/RusHydro/PskOnline.Plugins.RusHydro/PskOnline.Server.Plugins.RusHydro.csproj src/Server/Plugins/RusHydro/PskOnline.Plugins.RusHydro/
COPY src/Components/DotNetRtfWriter/DotNetRtfWriter.Std/DotNetRtfWriter.Std.csproj src/Components/DotNetRtfWriter/DotNetRtfWriter.Std/
COPY src/Methods/Svmr/PskOnline.Methods.Svmr.Processing/PskOnline.Methods.Svmr.Processing.csproj src/Methods/Svmr/PskOnline.Methods.Svmr.Processing/
COPY src/Methods/PskOnline.Methods.Processing/PskOnline.Methods.Processing.csproj src/Methods/PskOnline.Methods.Processing/
COPY src/Methods/PskOnline.Methods.ObjectModel/PskOnline.Methods.ObjectModel.csproj src/Methods/PskOnline.Methods.ObjectModel/
COPY src/Components/PskOnline.Math/PskOnline.Math.csproj src/Components/PskOnline.Math/
COPY src/Methods/Svmr/PskOnline.Methods.Svmr.ObjectModel/PskOnline.Methods.Svmr.ObjectModel.csproj src/Methods/Svmr/PskOnline.Methods.Svmr.ObjectModel/
COPY src/Methods/Hrv/PskOnline.Methods.Hrv.ObjectModel/PskOnline.Methods.Hrv.ObjectModel.csproj src/Methods/Hrv/PskOnline.Methods.Hrv.ObjectModel/
COPY src/Methods/Hrv/PskOnline.Methods.Hrv.Processing/PskOnline.Methods.Hrv.Processing.csproj src/Methods/Hrv/PskOnline.Methods.Hrv.Processing/
COPY src/Server/Plugins/RusHydro/PskOnline.Server.Plugins.Rushydro.DAL/PskOnline.Server.Plugins.RusHydro.DAL.csproj src/Server/Plugins/RusHydro/PskOnline.Server.Plugins.Rushydro.DAL/
COPY src/Server/PskOnline.Server.DAL/PskOnline.Server.DAL.csproj src/Server/PskOnline.Server.DAL/
RUN dotnet restore src/Server/PskOnline.Server.Service/PskOnline.Server.Service.csproj
COPY . .
WORKDIR /src/src/Server/PskOnline.Server.Service
RUN dotnet build PskOnline.Server.Service.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PskOnline.Server.Service.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PskOnline.Server.Service.dll"]
