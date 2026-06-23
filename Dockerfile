# Stage 1: Build ứng dụng bằng .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Sao chép file project và khôi phục các thư viện (NuGet packages)
COPY BanNoiThat/*.csproj ./BanNoiThat/
RUN dotnet restore BanNoiThat/BanNoiThat.csproj

# Sao chép toàn bộ mã nguồn và xuất bản ứng dụng (Publish) dạng Release
COPY . ./
RUN dotnet publish BanNoiThat/BanNoiThat.csproj -c Release -o out

# Stage 2: Môi trường chạy ứng dụng (ASP.NET Core 10.0 Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Cấu hình cổng mạng (Port) để Render có thể kết nối
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "BanNoiThat.dll"]
