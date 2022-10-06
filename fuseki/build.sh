#!/bin/bash

set -e

SCRIPT_LOCATION=$(dirname $0)
BASE_LOCATION=$(dirname $SCRIPT_LOCATION)

BUILD_LOCATION="$BASE_LOCATION/build"

DOCKER_REPO="https://repo1.maven.org/maven2/org/apache/jena/jena-fuseki-docker" #/4.4.0/jena-fuseki-docker-4.4.0.zip"
FUSEKI_VER="4.3.2"
set_image_url() {
	IMAGE_URL="$DOCKER_REPO/$FUSEKI_VER/jena-fuseki-docker-$FUSEKI_VER.zip"
	FUSEKI_LOCATION="$BUILD_LOCATION/jena-fuseki-docker-$FUSEKI_VER"
	ZIP_LOCATION="$FUSEKI_LOCATION.zip"
}
set_image_url ;

# IMAGE_URL='https://repo1.maven.org/maven2/org/apache/jena/jena-fuseki-docker/4.3.2/jena-fuseki-docker-4.3.2.zip'

usage()
{
	echo "Usage: "$0' [ -V | --fuseki-version [ 4.3.2 (default) | 4.4.0 ] ]
		 		 	  [ -v | --verbose ]
					  [ --dry-run ]'
	exit 2
}

PARSED_ARGS=$(getopt -a -n $0 -o V:v,h --long fuseki-version:,dry-run,verbose -- "$@")
VALID_ARGUMENTS=$?
if [ "$VALID_ARGUMENTS" != "0" ]; then
	usage
fi

eval set -- "$PARSED_ARGS"
ORIG_ARGS=$@


while :
do
	case "$1" in
		--) shift ; break ;;
		--dry-run) DRY_RUN=1 ; shift ;;
		-v | --verbose) VERBOSE=1 ; shift ;;
		-V | --fuseki-version)
			case "$2" in
				"4.3.2" | "4.3") FUSEKI_VER="4.3.2" ;;
				"4.4.0" | "4.4") FUSEKI_VER="4.4.0" ;;
				*) echo 'unsupported fuseki version (-V): '"$2" ; usage ; break ;;
			esac ;
			set_image_url ;
			shift 2 ;;
		-h | --help) usage; break ;;
	esac
done

if [ ! -z ${VERBOSE+x} ]; then
	echo "Fuseki version: $FUSEKI_VER" ;
	echo "Image download url: $IMAGE_URL" ;
	echo "Zip location: $ZIP_LOCATION" ;
	echo "Fuseki location: $FUSEKI_LOCATION" ;
fi


if [ ! -z ${DRY_RUN+x} ]; then
	echo "[DRY RUN] args were: $ORIG_ARGS" ;
	exit 0 ;
fi

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
[ -d "$BUILD_LOCATION" ] && rm -r $BUILD_LOCATION
mkdir -p $BUILD_LOCATION

#download
curl "$IMAGE_URL" -o "$ZIP_LOCATION"
echo $(cat $(basename "$ZIP_LOCATION.md5")) "$ZIP_LOCATION" | md5sum -c
unzip $ZIP_LOCATION -d $BUILD_LOCATION

#modify
cp entrypoint.sh $FUSEKI_LOCATION -v
cp Dockerfile $FUSEKI_LOCATION -v
cp -r config $FUSEKI_LOCATION/config -v

#build
docker build --force-rm --build-arg JENA_VERSION=$FUSEKI_VER -t spinefuseki $FUSEKI_LOCATION
