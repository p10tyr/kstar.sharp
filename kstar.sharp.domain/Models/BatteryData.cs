using kstar.sharp.domain.Enums;
using kstar.sharp.domain.Extensions;
using System;

namespace kstar.sharp.domain.Models
{
    public class BatteryData : InverterDataBase
    {


        public override string ToString()
        {
            //            string toString = String.Format(@"
            //Bat1:{0}W {1}V {2}A {3}% {4} 
            //Bat2:{5}W {6}V {7}A {8}% {9} 
            //Bat3:{10}W {11}V {12}A {13}% {14}
            //Bat4:{15}W {16}V {17}A {18}% {19}",
            //                Batter1Power, Vbat1, Ibat1, Cbat1, BatMode1,
            //                Batter2Power, Vbat2, Ibat2, Cbat2, BatMode2, 
            //                Batter3Power, Vbat3, Ibat3, Cbat3, BatMode3,
            //                Batter4Power, Vbat4, Ibat4, Cbat4, BatMode4
            //            );

            string toString = String.Format(@"
Bat1:{3}% {0}W {4}   {1}V {2}A",
    Battery1Power, Vbat1, Ibat1, Cbat1, BatMode1
);

            return toString;
        }


        public decimal Battery1Power { get { return BatMode1 == BatteryMode.CHARGE ? Decimal.Multiply(Vbat1, Ibat1) : Decimal.Multiply(Decimal.Multiply(Vbat1, Ibat1), -1); } }
        public decimal Battery2Power { get { return BatMode2 == BatteryMode.CHARGE ? Decimal.Multiply(Vbat2, Ibat2) : Decimal.Multiply(Decimal.Multiply(Vbat2, Ibat2), -1); } }
        public decimal Battery3Power { get { return BatMode3 == BatteryMode.CHARGE ? Decimal.Multiply(Vbat3, Ibat3) : Decimal.Multiply(Decimal.Multiply(Vbat3, Ibat3), -1); } }
        public decimal Battery4Power { get { return BatMode4 == BatteryMode.CHARGE ? Decimal.Multiply(Vbat4, Ibat4) : Decimal.Multiply(Decimal.Multiply(Vbat4, Ibat4), -1); } }

        public decimal Battery1Amp { get { return Decimal.Round(Ibat1, 2); } }
        public decimal Battery2Amp { get { return Decimal.Round(Ibat2, 2); } }
        public decimal Battery3Amp { get { return Decimal.Round(Ibat3, 2); } }
        public decimal Battery4Amp { get { return Decimal.Round(Ibat4, 2); } }

        public Int32 Battery1Charge { get { return Convert.ToInt32(Cbat1); } }
        public Int32 Battery2Charge { get { return Convert.ToInt32(Cbat2); } }
        public Int32 Battery3Charge { get { return Convert.ToInt32(Cbat3); } }
        public Int32 Battery4Charge { get { return Convert.ToInt32(Cbat4); } }

        public decimal Battery1Volt { get { return Decimal.Round(Vbat1, 1); } }
        public decimal Battery2Volt { get { return Decimal.Round(Vbat2, 1); } }
        public decimal Battery3Volt { get { return Decimal.Round(Vbat3, 1); } }
        public decimal Battery4Volt { get { return Decimal.Round(Vbat4, 1); } }






        private decimal Vbat1 = 0;
        private decimal Vbat2 = 0;
        private decimal Vbat3 = 0;
        private decimal Vbat4 = 0;

        private decimal Ibat1 = 0;
        private decimal Ibat2 = 0;
        private decimal Ibat3 = 0;
        private decimal Ibat4 = 0;

        private decimal Cbat1 = 0;
        private decimal Cbat2 = 0;
        private decimal Cbat3 = 0;
        private decimal Cbat4 = 0;

        private BatteryMode BatMode1 = BatteryMode.UNKNOWN;
        private BatteryMode BatMode2 = BatteryMode.UNKNOWN;
        private BatteryMode BatMode3 = BatteryMode.UNKNOWN;
        private BatteryMode BatMode4 = BatteryMode.UNKNOWN;


        public BatteryData(string[] HEXData)
            : base(HEXData)
        {

            try
            {
                Vbat1 = Decimal.Divide((HEXData[17] + HEXData[18]).HexToDecimal(), 10);
                Ibat1 = Decimal.Divide((HEXData[25] + HEXData[26]).HexToDecimal(), 10);
                Cbat1 = HEXData[33].HexToDecimal();
                BatMode1 = (BatteryMode)HEXData[37].HexToDecimal(); //2 discharge //3 charge
            }
            catch (Exception)
            {
                //throw;
            }


            try
            {
                Vbat2 = Decimal.Divide((HEXData[19] + HEXData[20]).HexToDecimal(), 10);
                Ibat2 = Decimal.Divide((HEXData[27] + HEXData[28]).HexToDecimal(), 10);
                Cbat2 = HEXData[34].HexToDecimal();
                BatMode2 = (BatteryMode)HEXData[38].HexToDecimal();
            }
            catch (Exception)
            {

                //throw;
            }

            try
            {

                Vbat3 = Decimal.Divide((HEXData[21] + HEXData[22]).HexToDecimal(), 10);
                Ibat3 = Decimal.Divide((HEXData[29] + HEXData[30]).HexToDecimal(), 10);
                Cbat3 = HEXData[35].HexToDecimal();
                BatMode3 = (BatteryMode)HEXData[39].HexToDecimal();
            }
            catch (Exception)
            {

                //throw;
            }

            try
            {
                Vbat4 = Decimal.Divide((HEXData[23] + HEXData[24]).HexToDecimal(), 10);
                Ibat4 = Decimal.Divide((HEXData[31] + HEXData[32]).HexToDecimal(), 10);
                Cbat4 = HEXData[36].HexToDecimal();
                BatMode4 = (BatteryMode)HEXData[40].HexToDecimal();
            }
            catch (Exception)
            {

                //throw;
            }


        }


    }
}
