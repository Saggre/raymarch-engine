// Created by Sakri Koskimies (Github: Saggre) on 07/11/2019

namespace EconSim.EMath
{
    public struct Byte4
    {
        private byte a;
        private byte b;
        private byte c;
        private byte d;

        public Byte4(byte a, byte b, byte c, byte d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        #region Getters and Setters ABCD

        public byte A
        {
            get => a;
            set => a = value;
        }

        public byte B
        {
            get => b;
            set => b = value;
        }

        public byte C
        {
            get => c;
            set => c = value;
        }

        public byte D
        {
            get => d;
            set => d = value;
        }

        #endregion

        #region Getters and Setters XYZW

        public byte X
        {
            get => a;
            set => a = value;
        }

        public byte Y
        {
            get => b;
            set => b = value;
        }

        public byte Z
        {
            get => c;
            set => c = value;
        }

        public byte W
        {
            get => d;
            set => d = value;
        }

        #endregion

    }
}