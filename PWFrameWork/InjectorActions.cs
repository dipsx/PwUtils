using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PWFrameWork.Consts;

namespace PWFrameWork {
    public class InjectorActions {
        private int _pid { get; }
        private ASM _asm { get; }

        public InjectorActions(int pid) {
            _pid = pid;
            _asm = new ASM();
        }

        //pushad
        //mov eax, BaseAddr
        //mov eax, dword ptr [eax]
        //mov eax, dword ptr [eax+$1C]
        //mov esi, dword ptr [eax+$34] //Y     //20
        //mov ecx, dword ptr [esi+$1500]//Y
        //push 1
        //call CallAddress1
        //lea edx, dword ptr [esp+$20]     //18
        //mov ebx, eax
        //push edx
        //push flying
        //mov ecx, ebx
        //call CallAddress2
        //mov ecx, dword ptr [esi+$1500]  //Y
        //mov eax, x
        //mov dword ptr[ebx+$20], eax
        //mov eax, z
        //mov dword ptr[ebx+$24], eax
        //mov eax, y
        //mov dword ptr[ebx+$28], eax
        //push 0
        //push ebx
        //push 1
        //call CallAddress3
        //popad

        public void WalkTo(Single coord_X, Single coord_Y, Single coord_Z, int isFly = 1) {
            int Address1 = 0x4C7AB0;
            int Address2 = 0x4CDFB0;
            int Address3 = 0x4C80C0;

            _asm.Pushad();
            _asm.Mov_EAX(Offsets.BA);
            _asm.Mov_EAX_DWORD_Ptr_EAX();
            _asm.Mov_EAX_DWORD_Ptr_EAX_Add(0x1c);
            _asm.Mov_ESI_DWORD_Ptr_EAX_Add(0x34);
            _asm.Mov_ECX_DWORD_Ptr_ESI_Add(0x1500);
            _asm.Push68(1);
            _asm.Mov_EDX(Address1);
            _asm.Call_EDX();

            _asm.Lea_EDX_DWORD_Ptr_ESP_Add(0x20);
            _asm.Mov_EBX_EAX();
            _asm.Push_EDX();
            _asm.Mov_EDX(isFly);
            _asm.Push_EDX();
            _asm.Mov_ECX_EBX();
            _asm.Mov_EDX(Address2);
            _asm.Call_EDX();

            _asm.Mov_ECX_DWORD_Ptr_ESI_Add(0x1500);
            _asm.Mov_EAX(coord_X);
            _asm.Mov_EDX_EBX();
            _asm.Add_EDX(0x20);
            _asm.Mov_DWORD_Ptr_EDX_EAX();
            _asm.Mov_EAX(coord_Z);
            _asm.Mov_EDX_EBX();
            _asm.Add_EDX(0x24);
            _asm.Mov_DWORD_Ptr_EDX_EAX();
            _asm.Mov_EAX(coord_Y);
            _asm.Mov_EDX_EBX();
            _asm.Add_EDX(0x28);
            _asm.Mov_DWORD_Ptr_EDX_EAX();

            _asm.Push68(0);
            _asm.Push_EBX();
            _asm.Push68(1);
            _asm.Mov_EDX(Address3);
            _asm.Call_EDX();
            _asm.Popad();
            _asm.Ret();


            _asm.RunAsm(_pid, 0);
        }


        public void TargetMob(int wid) {
            _asm.Pushad();
            _asm.Mov_EDI(wid);
            _asm.Mov_EBX(0x0080DED0);
            _asm.Mov_EAX(Offsets.BA);
            _asm.Push_EDI();
            _asm.Mov_ECX_DWORD_Ptr_EAX_Add(0x20);
            _asm.Add_ECX(0xF4);
            _asm.Call_EBX();
            _asm.Popad();
            _asm.Ret();

            _asm.RunAsm(_pid, 0);
        }

    }
}
