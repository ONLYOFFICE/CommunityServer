'	flXHR 1.0.4 (VB Binary Helpers) <http://flxhr.flensed.com/>
'	Copyright (c) 2008 Kyle Simpson, Getify Solutions, Inc.
'	This software is released under the MIT License <http://www.opensource.org/licenses/mit-license.php>
'
'	====================================================================================================

Function flXHR_vb_BinaryToString(obj)
  Dim I,S
  Dim J
  For I = 1 to LenB(obj)
    J = AscB(MidB(obj,I,1))
    If J = 0 Then
      S = S & ""
    Else
      S = S & Chr(J)
    End If
  Next
  flXHR_vb_BinaryToString = S
End Function

Function flXHR_vb_SizeOfBytes(obj)
  Dim I
  I = LenB(obj)
  flXHR_vb_SizeOfBytes = I
End Function

Function flXHR_vb_StringToBinary(str)
    dim binobj
    dim ahex(),oparser,oelem
    redim ahex(len(str)-1)
    for i=0 to len(str)-1
        ahex(i)=right("00" & hex(asc(mid(str,i+1,1))),2)
    next
    set oparser=createobject("msxml2.domdocument")
    with oparser
        set oelem=.createElement("x")
        oelem.datatype="bin.hex"
        oelem.text=join(ahex,"")
        binobj=oelem.nodetypedvalue
    end with
    set oelem=nothing
    set oparser=nothing
    flXHR_vb_StringToBinary=binobj
End Function
