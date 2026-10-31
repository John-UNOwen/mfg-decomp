import struct, os, ctypes
from ctypes import *
OUT = r"C:\mfg\build\_tmp_audit_face"
g = open(os.path.join(OUT,"face_blob.bin"),"rb").read()
dc = windll.LoadLibrary("d3dcompiler_47")
D3DDisassemble = dc.D3DDisassemble
D3DDisassemble.restype = c_long
D3DDisassemble.argtypes = [c_void_p, c_size_t, c_uint, c_char_p, POINTER(c_void_p)]
data = g[1534:1534+2732]
blob = c_void_p()
hr = D3DDisassemble(c_char_p(data), len(data), 0, b"", byref(blob))
p = blob.value
q = struct.unpack_from("<4Q", string_at(p, 32))
txt = string_at(q[3], q[2]).decode("latin-1")
open(os.path.join(OUT,"asm02_sample.txt"),"w").write(txt)
print(txt[:1500])
