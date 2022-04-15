#! /bin/bash

DESCRIBE=`git describe --tags --long --match v[0-9]\*`
#DESCRIBE=`git describe --tags --long --match v[0-9]\* $(git rev-list --tags --max-count=1)`

# increment the build number (ie 115 to 116)
VERSION=`echo $DESCRIBE | awk '{split($0,a,"-"); print a[1]}'`
BUILD=`echo $DESCRIBE | awk '{split($0,a,"-"); print a[2]}'`

if [[ "${DESCRIBE}" =~ ^[A-Fa-f0-9]+$ ]]; then
    VERSION="0.0.0"
    BUILD=`git rev-list HEAD --count`
    BUILD=${DESCRIBE}
fi

if [ "${BUILD}" = "" ]; then
    BUILD='0'
fi

echo ${VERSION:1}.${BUILD}
