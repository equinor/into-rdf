#!/bin/bash

set -e

SCRIPT_LOCATION=$(dirname $0)
BASE_LOCATION=$(dirname $SCRIPT_LOCATION)

BUILD_LOCATION="$BASE_LOCATION/build"
FUSEKI_LOCATION="$BUILD_LOCATION/jena-fuseki-docker-4.3.1"
ZIP_LOCATION="$FUSEKI_LOCATION.zip"

#checks
commands=("docker" "unzip")
for cmd in ${commands[@]}; do
    if ! command -v $cmd &> /dev/null; then
        >&2 echo "$cmd must be installed"
        exit 1
    fi
done

if [ ! -w $HOST_DB_LOCATION ]; then
    >&2 echo " $HOST_DB_LOCATION must exist and be writable"
    exit 2
fi


#clean
rm -r $BUILD_LOCATION
mkdir -p $BUILD_LOCATION

#download
curl https://repo1.maven.org/maven2/org/apache/jena/jena-fuseki-docker/4.3.1/jena-fuseki-docker-4.3.1.zip -o "$ZIP_LOCATION"
echo $(cat $SCRIPT_LOCATION/fuseki_4_3_1_md5sum) "$ZIP_LOCATION" | md5sum -c
unzip $ZIP_LOCATION -d $BUILD_LOCATION

#modify
cp entrypoint.sh $FUSEKI_LOCATION -v
cp reasoning_config.ttl $FUSEKI_LOCATION -v
cp no_reasoning_config.ttl $FUSEKI_LOCATION -v
cp Dockerfile $FUSEKI_LOCATION -v

#build
docker build --force-rm --build-arg JENA_VERSION=4.3.1 -t fuseki $FUSEKI_LOCATION
