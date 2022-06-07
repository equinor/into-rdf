#!/bin/bash

set -e

while getopts e:s: flag
do
    case "${flag}" in
        e) environment=${OPTARG};;
    esac
    case "${flag}" in
        s) sku=${OPTARG};;
    esac
done

if [ -z $environment ]; then
    >&2 echo "environment must be provided using -e";
    exit 1;
fi
if [ -z $sku ]; then
    sku="B1"
fi

group="${environment}-spinesplinter";
location="norwayeast";

az group create --name $group --location $location

echo "Running Bicep in resourceGroup: $group.";

az deployment group create \
--resource-group $group \
--template-file ./setupEnv.bicep \
--parameters \
resourcePrefix=$group \
sku=$sku
