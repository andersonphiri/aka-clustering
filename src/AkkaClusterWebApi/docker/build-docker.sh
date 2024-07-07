#!/usr/bin/env bash
dotnet publish ../src/AkkaClusterWebApi.App/AkkaClusterWebApi.App.csproj --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer