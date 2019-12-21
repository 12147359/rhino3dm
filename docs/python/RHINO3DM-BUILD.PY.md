# Build rhino3dm.py for yourself
### Get The Source

This repo uses [OpenNURBS](https://github.com/mcneel/opennurbs) and [pybind11](https://github.com/pybind/pybind11) as submodules, so you need to run another git command after you have cloned. `cd` into the new repository directory and run
  * `git submodule update --init`

## Install the Tools

CMake 3.12.1 is the minimum required CMake version.

* Mac
  * Install Homebrew (https://brew.sh/)
  * `brew install python2 cmake` (for Python 2.7 compile)
  * `brew install python3 cmake` (for Python 3.7 compile)
* Windows
  * This project uses Visual Studio 2017
  * Install the flavor of CPython that you prefer to work with from python.org
  * Install CMake (https://cmake.org/download/) and make sure that cmake.exe is added to the path
* Linux
  * Tested with Clang 3.8.0 on Linux Mint 18.3
  * Install CMake 3.12.1
  * `sudo aptitude install python2 python3 python2-dev python3-dev uuid uuid-dev`

## Compile

* (All platforms) run `python setup.py bdist` in the root directory to compile and configure. The library will compile for the version of python that you are executing.

* (Windows) If you are on Windows, you can create a Visual Studio project file for editing and compiling code by running the `build_python_project.py` script in the `src` directory. This is the easiest way to add new code to the project.

## Compile and install a development version

* (All platforms) Alternatively, running `pip install -e .` from the root of the repository will compile the Python extension, copy it in `/src/rhino3dm`, and link `/src/rhino3dm` in your installed packages. Rebuilds will be faster than building a binary distribution. To rebuild after modifying the C++ source files, run `python setup.py develop`.

## Test

* `cd build_{pyver}/stage` and start `python`
```
>>> from rhino3dm import *
>>> center = Point3d(1,2,3)
>>> arc = Arc(center, 10, 1)
>>> nc = arc.ToNurbsCurve()
>>> start = nc.PointAtStart
>>> print(start)
```
