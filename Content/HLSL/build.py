import os, sys, subprocess
import glob
for fpath in glob.glob(sys.argv[1] + "\\*.fx"):
    output = sys.argv[3] + "\\" + fpath.split("\\")[-1].split(".")[0] + ".mgfx"
    headers = glob.glob(sys.argv[1]+ "\\*.fxh")
    lastModified = max([os.stat(path).st_mtime for path in headers + [fpath]])
    if not os.path.exists(output) or lastModified >= os.stat(output).st_mtime:
        if not os.path.exists(os.path.dirname(output)):
            os.makedirs(os.path.dirname(output))
        cmd = sys.argv[2] + " " + \
        fpath + " " + \
        output + \
        " /Profile:DirectX_11"
        print "Updating:", cmd
        process = subprocess.Popen(cmd.split(),stderr=subprocess.PIPE)
        err = process.stderr.read().strip()
        process.communicate()
        if process.returncode:
            raise Exception(str(err))
