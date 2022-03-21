#!/bin/bash

set -e

while getopts t:e:c: flag
do
    case "${flag}" in
        t) type=${OPTARG};;
        e) environment=${OPTARG};;
        c) clientId=${OPTARG};;
    esac
done


if [ -z $type ]; then
    >&2 echo "type must be provided using -t";
    exit 1;
fi

if [ -z $environment ]; then
    >&2 echo "environment must be provided using -e";
    exit 1;
fi

if [ -z $clientId ]; then
    >&2 echo "clientId must be provided using -c";
    exit 1;
fi

# cd to ${BASH_SOURCE%/*} to make sure we are in same folder as script
# in order to reference ./setupEnv.bicep correctly.
# see http://mywiki.wooledge.org/BashFAQ/028
if [[ $BASH_SOURCE = */* ]]; then
    cd -- "${BASH_SOURCE%/*}/" || (>&2 echo "Unknown bash source" && exit 1);
fi

group="${environment}-${type}";
echo "Creating resource in resourceGroup: $group. Using client id $clientId";

az group create --name $group --location norwayeast
az deployment group create --resource-group $group --template-file ./setupEnv.bicep --parameters fusekiType=$type env=$environment clientId=$clientId
