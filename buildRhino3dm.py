import os, platform, sys, glob
from shutil import copyfile, copytree, rmtree, copy

windows_build = os.name == 'nt'
mac_build = platform.system() == 'Darwin'

# all compilation and staging occurs in the build directory
if not os.path.exists("build"):
    os.mkdir("build")

def compilebinaries():
    """ compile for the platform we are running on """
    if windows_build:
        print "Compiling for Windows"
        msbuildpath = r'C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe'
        print "Compiling for Windows (32-bit)"
        os.system('"{}" rhino3dm_py.sln /p:Configuration=Release;Platform=Win32'.format(msbuildpath))
        print "Compiling for Windows (64-bit)"
        os.system('"{}" rhino3dm_py.sln /p:Configuration=Release;Platform=x64'.format(msbuildpath))
    if mac_build:
        os.chdir("build")
        PYTHON_LIBRARY="/System/Library/Frameworks/Python.framework/Versions/2.7/lib/libpython2.7.dylib"
        #PYTHON_INCLUDE_DIR="/System/Library/Frameworks/Python.framework/Versions/2.7/include/python2.7"
        PYTHON_LIBRARY="/usr/local/Cellar/python@2/2.7.15_1/Frameworks/Python.framework/Versions/2.7/lib/libpython2.7.dylib"
        PYTHON_INCLUDE_DIR="/usr/local/Cellar/python@2/2.7.15_1/Frameworks/Python.framework/Versions/2.7/Headers"
        BUILD_TYPE="RELEASE"
        args = "-DPYTHON_LIBRARY={} -DPYTHON_INCLUDE_DIR={} -DCMAKE_BUILD_TYPE={} .. && make".format(PYTHON_LIBRARY, PYTHON_INCLUDE_DIR, BUILD_TYPE)
        os.system("cmake "+ args)
        os.chdir("..")

def createwheel():
    """stage files and generate wheel for distribution"""
    current_dir = os.path.abspath(".")
    staging_dir = os.path.abspath("build/staging")
    if os.path.exists(staging_dir):
        rmtree(staging_dir)

    os.chdir("build")
    os.mkdir(staging_dir)
    os.chdir("..")
    copytree("pysrc/rhino3dm", staging_dir +"/rhino3dm")
    if windows_build:
        copyfile("build/Release/Win32/_rhino3dm_win32.pyd", staging_dir + "/rhino3dm/_rhino3dm_win32.pyd")
        copyfile("build/Release/x64/_rhino3dm_win64.pyd", staging_dir + "/rhino3dm/_rhino3dm_win64.pyd")
    if mac_build:
        copyfile("build/_rhino3dm.so", staging_dir + "/rhino3dm/_rhino3dm.so")

    copyfile("LICENSE", staging_dir + "/LICENSE")
    copyfile("pysrc/README.md", staging_dir + "/README.md")
    copyfile("pysrc/MANIFEST.in", staging_dir + "/MANIFEST.in")
    copyfile("pysrc/setup.py", staging_dir + "/setup.py")
    os.chdir(staging_dir)
    options = ""
    #platform is found wit distutils.util.get_platform()
    if windows_build:
        options = "--plat-name=win32"
    if mac_build:
        options = "--python-tag=cp27 --plat-name=macosx-10.13-x86_64"
    os.system(sys.executable + " setup.py bdist_wheel " + options)
    os.chdir(current_dir)
    if not os.path.exists("artifacts"):
        os.mkdir("artifacts")
    for file in glob.glob(staging_dir + "/dist/*.whl"):
        print file
        copy(file, "artifacts" )


compilebinaries()
createwheel()
