# rhino3dm.py
CPython package based on OpenNURBS with a RhinoCommon style

## Install from pip
`pip install rhino3dm`
If you get an error, you may need to run `pip install --user rhino3dm`

### Supported platforms
* Python 2.7 - Windows (32 and 64 bit)
* Python 3.7 - Windows (32 and 64 bit)
* Python 2.7 - OSX (installed through homebrew)
* Python 3.7 - OSX (installed through homebrew)
* Other distributions are possible, just let us know where you need this package to run

---

## Build it yourself

### Get The Source

This repo uses OpenNURBS and pybind11 as submodules, so you need to run another git command after you have cloned. `cd` into the new repository directory and run
  * `git submodule update --init`

## Install the Tools

* Mac
  * Install Homebrew (https://brew.sh/)
  * `brew install python2 cmake` (for Python 2.7 compile)
  * `brew install python3 cmake` (for Python 3.7 compile)
* Windows
  * This project uses Visual Studio 2017
  * Install the flavor of CPython that you prefer to work with from python.org
  * Install CMake (https://cmake.org/download/) and make sure that cmake.exe is added to the path

## Compile

* (All platforms) run the `build_rhino3dm.py` script to compile and configure
  * Use Python 2.7 on `build_rhino3dm.py` to compile a 2.7 compatible package
  * Use Python 3.7 on `build_rhino3dm.py` to compile a 3.7 compatible package

## Test

* `cd artifacts` and start `python`
```
>>> from rhino3dm import *
>>> center = Point3d(1,2,3)
>>> arc = Arc(center, 10, 1)
>>> nc = arc.ToNurbsCurve()
>>> start = nc.PointAtStart
>>> print start
```
