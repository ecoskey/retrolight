using System;
using UnityEngine;

namespace Retrolight.Data {
    public struct Matrix2x3 : IEquatable<Matrix2x3> {
        private float r0c0;
        private float r1c0;
        private float r0c1;
        private float r1c1;
        private float r0c2;
        private float r1c2;
        
        public float this[int index] {
            get => index switch {
                0 => r0c0,
                1 => r1c0,
                2 => r0c1,
                3 => r1c1,
                4 => r0c2,
                5 => r1c2,
                _ => r0c0,
            };
            set {
                switch (index) {
                    case 0: 
                        r0c0 = value;
                        break;
                    case 1: 
                        r1c0 = value;
                        break;
                    case 2: 
                        r0c1 = value;
                        break;
                    case 3: 
                        r1c1 = value;
                        break;
                    case 4: 
                        r0c2 = value;
                        break;
                    case 5: 
                        r1c2 = value;
                        break;
                }
            }
        }

        public bool Equals(Matrix2x3 other) {
            return r0c0.Equals(other.r0c0)
                && r1c0.Equals(other.r1c0)
                && r0c1.Equals(other.r0c1)
                && r1c1.Equals(other.r1c1)
                && r0c2.Equals(other.r0c2)
                && r1c2.Equals(other.r1c2);
        }

        public override bool Equals(object obj) {
            return obj is Matrix2x3 other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(r0c0, r1c0, r0c1, r1c1, r0c2, r1c2);
        }
    }
}