###############################################################################
#
#   Modified from https://github.com/huytd/adb.py
#
###############################################################################

import sys, os, subprocess, time, signal, random
from time import gmtime, strftime
import shlex
from multiprocessing import Process
from multiprocessing.pool import ThreadPool

class ADB( object ):
    m_defaultDevice = ""
    m_showDebugInfo = False


###############################################################################
# BE AWARE (Python's bug):
# os.popen() ONLY works for the 1st process for parallelization
# Subsequent calls will be in sequential order.
#
    def call( self, command ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        command_result = ''
        command_text = 'adb %s' % command
        print "command == ", command_text
        results = os.popen( command_text, "r" )
        while True:
            line = results.readline()
            if not line: break
            command_result += line
        results.close()
        return command_result


###############################################################################
# We will pick the 1st device as the default one for use.
# ----- This method needs to be called FIRST -----
#
    def devices( self ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        result = self.call( "devices" )
        devices = result.partition( '\n' )[2].replace( '\n', '' ).split( '\tdevice' )
        print str(devices), len(devices)
        if len(devices) >= 2:
            self.m_defaultDevice = devices[0]

        print( "Total {} devices found. Choose {} as the default one.".format( len(devices), devices[0] ) )
        return [device for device in devices if len(device) >= 2]


###############################################################################
# Reset the environment
#
    def startNewSession( self ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        self.call( "kill-server" )
        time.sleep( 1 )
        self.call( "start-server" )
        self.call( "wait-for-device" )
        self.devices( )


###############################################################################
#
    def upload( self, fr, to, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " push " + fr + " " + to )
        return result


###############################################################################
#
    def get( self, fr, to, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " pull " + fr + " " + to )
        return result


###############################################################################
#
    def install( self, param, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = ""
        data = param.split()
        print "--- install = ", data.count, len(data), self.m_defaultDevice
        if not os.path.isfile( data[0] ):
            raise NameError( "Bad Apk: " + data[0] + " NOT exist" )

        if len(data) == 1:
            result = self.call( "-s " + device + " install -r " + data[0] )
        elif len(data) == 2:
            result = self.call( "-s " + device + " install -r " + data[0] + " " + data[1] )

        return self.checkResult( result, "success" )


###############################################################################
#
    def uninstall( self, package, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " shell pm uninstall " + package )
        return self.checkResult( result, "success" )


###############################################################################
#
    def clearLogcat( self, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " logcat -c " )


###############################################################################
#
    def logcat( self, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " logcat " )


###############################################################################
#
    def clearData( self, package, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " shell pm clear " + package )
        return self.checkResult( result, "success" )


###############################################################################
#
    def shell( self, command, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " shell " + command )
        return self.checkResult( result, "success" )


###############################################################################
#
    def kill( self, package, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " kill " + package )
        return self.checkResult( result, "success" )


###############################################################################
#
    def start( self, app, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        pack = app.split()
        result = "Nothing to run"
        if pack.length == 1:
            result = self.call( "-s " + device + " shell am start " + pack[0] )    
        elif pack.length == 2:
            result = self.call( "-s " + device + " shell am start " + pack[0] + "/." + pack[1] )
        elif pack.length == 3:
            result = self.call( "-s " + device + " shell am start " + pack[0] + " " + pack[1] + "/." + pack[2] )

        return self.checkResult( result, "success" )


###############################################################################
#
    def screen( self, res, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " am display-size " + res )
        return self.checkResult( result, "success" )


###############################################################################
#
    def dpi( self, dpi, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        result = self.call( "-s " + device + " am display-density " + dpi )
        return self.checkResult( result, "success" )


###############################################################################
#
    def screenRecord( self, param, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        params = param.split()
        if params.length == 1:
            result = self.call( "-s " + device + " shell screenrecord " + params[0] )
        elif params.length == 2:
            result = self.call( "-s " + device + " shell screenrecord --time-limit " + params[0] + " " + params[1] )

        return self.checkResult( result, "success" )


###############################################################################
#
    def screenShot( self, output, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        self.call( "-s " + device + " shell screencap -p /sdcard/temp_screen.png" )
        self.get(  "-s " + device + " /sdcard/temp_screen.png", output )
        self.call( "-s " + device + " shell rm /sdcard/temp_screen.png" )


###############################################################################
# return the PID of app running on the device that we are talking to via JDWP
#
    def jdwp( self, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        strPID = ""
        jdwpProc, logFile = self.runProcess( "jdwp", device, "jdwp" )
        print "JDWP host process' PID is %s." % jdwpProc.pid

        # Let's read till end of file to get the last PID that corresponds to the test app we are interested in
        for line in open( logFile, "r" ):
            if line:
                print " --- find PID = ", line
                strPID = line

        time.sleep( 1 )
        if jdwpProc.poll() == None:
            print "JDWP process is still running. Let's stop it. JDWP PID on your host = ", jdwpProc.pid
            os.kill( jdwpProc.pid, signal.SIGTERM )
            #jdwpProc.terminate()

        return strPID, logFile


###############################################################################
# Input : device (string, default is m_defaultDevice)
# Return: PID (int) of the logcat process running on the host
#         logFile (string) that hosts the logcat messages
#
    def startLogcat( self, device=m_defaultDevice ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not device:
            raise NameError( "bad parameter: device name cannot be empty" )

        logcatProc, logFile = self.runProcess( "logcat", device, "logcat" )
        print "Logcat's PID is %s on host." % logcatProc.pid
        return logcatProc, logFile


###############################################################################
# Input : Proc (subProcess object) of the process running logcat on host
#         logFileName (string) to store console output
#         keepLogFile (boolean)
# Return: True if logcat process is terminated.
#         logFileName is deleted if keepLogFile is False
#
    def stopLogcat( self, proc, logFileName, keepLogFile = True ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if (not logFileName) or (not proc) or ( proc.pid <= 0 ):
            raise NameError( "bad parameter" )

        try:
            #time.sleep( 1 )
            #proc.terminate()
            os.kill( proc.pid, signal.SIGTERM )
            time.sleep( 1 )

            if keepLogFile == False:
                print "Clear temp log file ", logFileName
                os.remove( logFileName )

            return True
        except Exception as ex:
            print ex
            return False


###############################################################################
# Input : fullCommand (string) the command without "adb -s"
#         device (string, default is m_defaultDevice )
# Return: proc (Popen object)
#         tempLogFile (string) for logging console output
# NOTE:
#         It is the caller's responsibility to terminate the proc. !!!!!
#
    def runProcess( self, fullCommand, device = m_defaultDevice, tag = "" ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if (not device) or (not fullCommand):
            raise NameError( "empty parameter" )

        args = [ "adb", "-s", str(device) ]
        args.extend( fullCommand.split(" ") )
        tempLogFile = self.getRandomLogFileName( tag )

        proc = subprocess.Popen( args = args, stdout = open( tempLogFile, 'w' ) )
        print "Host process ", proc.pid, " is created to run command ", str( args )
        print "  stdout is directed to file ", tempLogFile
        return proc, tempLogFile


###############################################################################
# Input : cmd (string) ANY shell/DOS command you want to run in parallel
# Return: the stdout and stderr strings
#
# Python is weak in parallel processing, with tons of bugs in its os and subprocess modules,
# and this method lays the ground for async call, which is still buggy but at least works
#
    def call_proc( self, cmd ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        """ This runs in a separate thread. """
        #subprocess.call(shlex.split(cmd))  # This will block until cmd finishes
        #print "--- cmd = ", cmd
        p = subprocess.Popen( shlex.split( cmd ), stdout = subprocess.PIPE, stderr = subprocess.PIPE )
        out, err = p.communicate()
        return ( out, err )


###############################################################################
# Input : result string array from JDWP call
# Return: 0 if it's empty or error, or PID of JDWP (the last item within the string)
#
    def getJdwpPid( self, jdwpResult ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if not jdwpResult:
            return 0

        pidsList = jdwpResult[0].split( "\r\n" )
        pid = None
        while ( len( pidsList ) > 1 ) and ( not pid ):
            pid = pidsList.pop()

        if len( pidsList ) == 1:
            pid = pidsList[0]

        if not pid:
            #print " --- cannot get JDWP Pid"
            return 0
        else:
            #print( " --- the corresponding JDWP process running on the device is {}".format( pid ) )
            return pid


###############################################################################
# Input : result string
# Return: True if "success" is found.
#
    def checkResult( self, result, expectedString ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        if result.lower().find( expectedString.lower() ) >= 0:
            return True
        else:
            return False


###############################################################################
# Input : tagName (string) for charcterize the type of log file, e.g. JDWP or logcat
# Return: a randomly generated file name (based on current date & time) for logging.
#
# Also convert the path to use forward slashes on Windows, the same way on Linux
# since Python could handle that properly.
#
    def getRandomLogFileName( self, tagName = "" ):
        self.printDebugInfo_Function( sys._getframe().f_code.co_name )

        tagStr = ""
        if tagName:
            tagStr = tagName + "_"

        tempLogFile = str( os.environ['TEMP'] ) + "/g4jtest_" + tagStr + strftime( "%Y%m%d_%H%M%S_", gmtime() ) + str( random.randrange( 0x1234FFFF ) ) + ".log"

        if( os.name == "nt" ):                                                  # convert path to Linux style
            tempLogFile = tempLogFile.replace( "\\", "/" )                      # as python could handle that properly on Windows

        return tempLogFile


###############################################################################
# Print out the name of the function under execution, in debug mode
# Input : functionName string
# Return: N/A
#
    def printDebugInfo_Function( self, functionName ):
        if self.m_showDebugInfo:
            print "\r\n------------------------------------------------------\r\n----- Entering Method {}\r\n".format( functionName )
