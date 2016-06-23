#!/bin/sh

DIR=$(readlink -f $(dirname $0))
BUILDDIR=$DIR/builddir
REPODIR=$DIR/repodir
SPECFILE=$DIR/onlyoffice.spec
BUILDMODE=-bb
PUBLISHMODE=develop
VERSION=1
CHECKPERMISSION=1

while [ "$1" != "" ]; do
	case $1 in
		-u | --unsafe )
			CHECKPERMISSION=0
			shift
		;;
	
		-b | --build )
			BUILD=1
			if [ "$2" = "install-only" ]; then
				BUILDMODE=-bi
				shift
			fi
		;;

		-r | --createrepo )
			REPO=1
			if [ "$2" != "" ]; then
				VERSION=$2
				shift
			fi
		;;

		-p | --publish )
			PUBLISH=1
			if [ "$2" = "develop" -o "$2" = "production" -o "$2" = "release" ]; then
				PUBLISHMODE=$2
				shift
			fi
		;;

		-? | -h | --help )
			echo "  Usage $0 [PARAMETER] [OPTIONS] [[PARAMETER] [OPTIONS], ...]"
			echo "    Parameters:"
			echo "      -b, --build         build package to $BUILDDIR directory"
			echo "      -r, --createrepo    create repository in $REPODIR directory"
			echo "      -p, --publish       publish repository to s3"
			echo "      -?, -h, --help      this help"
			echo "    Repo options:"
			echo "      version             package version"
			echo "    Build options:"
			echo "      install-only        install files to temp directory, do not package them"
			echo "    Publish options:"
			echo "      develop               publish package for develop"
			echo "      release                publish package for release"
			echo "      production          publish package for production"
			echo
			exit 0
		;;

		* )
			echo "Unknown parameter $1" 1>&2
			exit 1
		;;
	esac
	shift
done

[ -e $SPECFILE ] || { echo "$SPECFILE not found"; exit 1; }

[ "$CHECKPERMISSION" = "1" -a $(id -u) -eq 0 ] && { echo "Error: You shouldn't run rpmbuild as root"; exit 1; }

if [ "$BUILD" = "1" ]; then
	rm -rfv $BUILDDIR
	echo "Building package... "
	rpmbuild $BUILDMODE --define "_topdir $BUILDDIR" "$SPECFILE"
	[ $? -ne 0 ] && exit 1
fi

if [ "$REPO" = "1" ]; then
	rm -rfv $REPODIR
	mkdir -p $REPODIR/main
	mkdir $REPODIR/$VERSION
	for F in $BUILDDIR/RPMS/*; do
		ARCH=$(basename $F)
		echo "Creating repository ($ARCH)... "
		cp -rv $BUILDDIR/RPMS/$ARCH $REPODIR/main
		createrepo -v $REPODIR/main/$ARCH/
		cp -rv $BUILDDIR/RPMS/$ARCH $REPODIR/$VERSION
		createrepo -v $REPODIR/$VERSION/$ARCH/
		[ $? -ne 0 ] && exit 1
	done
fi

if [ "$PUBLISH" = "1" ]; then
	echo "Publishing ($PUBLISHMODE)... "
	if [ "$PUBLISHMODE" = "develop" ]; then
		S3PATH=s3://static.teamlab.com/testing/rpm-develop/
	elif [ "$PUBLISHMODE" = "release" ]; then
		S3PATH=s3://static.teamlab.com/testing/rpm-release/
	elif [ "$PUBLISHMODE" = "production" ]; then
		S3PATH=s3://static.teamlab.com/repo/centos/
	fi

	[ "$S3PATH" != "" ] && s3cmd --delete-removed --acl-public sync $REPODIR/ $S3PATH
fi
