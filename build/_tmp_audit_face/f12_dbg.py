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
print("hr", hex(hr & 0xffffffff), "blob", hex(blob.value or 0))
if hr == 0 and blob.value:
    p = blob.value
    # dump first 4 qwords of the object
    q = struct.unpack_from("<4Q", string_at(p, 32))
    print("obj qwords:", [hex(x) for x in q])
    # try COM calls with proper prototypes
    vt = struct.unpack_from("<4Q", string_at(q[0], 32))
    print("vtable slots:", [hex(x) for x in vt])
    fGet = WINFUNCTYPE(c_void_p, c_void_p)(vt[3])
    fSize = WINFUNCTYPE(c_size_t, c_void_p)(vt[4])
    buf = fGet(c_void_p(p)); n = fSize(c_void_p(p))
    print("buf", hex(buf or 0), "size", n)
    print(string_at(buf, min(n,300)).decode("latin-1"))
