
using System;
using UnityEngine;

namespace Retrolight.Data {
    public struct Matrix2x2 : IEquatable<Matrix2x2> {
        private float r0c0;
        private float r1c0;
        private float r0c1;
        private float r1c1;
        
        //constructor is row-major for better code readability
        public Matrix2x2(float r0c0, float r0c1, float r1c0, float r1c1) {
            this.r0c0 = r0c0;
            this.r0c1 = r0c1;
            this.r1c0 = r1c0;
            this.r1c1 = r1c1;
        }

        public static Matrix2x2 Identity = new Matrix2x2(
            1, 0,
            0, 1
        );

        public static Matrix2x2 Zero = new Matrix2x2(0, 0, 0, 0);

        public float this[int index] {
            get => index switch {
                0 => r0c0,
                1 => r1c0,
                2 => r0c1,
                3 => r1c1,
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
                }
            }
        }

        public static Matrix2x2 operator *(float lhs, Matrix2x2 rhs) {
            return new Matrix2x2 {
                r0c0 = lhs * rhs.r0c0,
                r1c0 = lhs * rhs.r1c0,
                r0c1 = lhs * rhs.r0c1,
                r1c1 = lhs * rhs.r1c1,
            };
        }

        public static Vector2 operator *(Matrix2x2 lhs, Vector2 rhs) {
            return new Vector2 (
                lhs.r0c0 * rhs.x + lhs.r1c0 * rhs.y, 
                lhs.r0c1 * rhs.x + lhs.r1c1 * rhs.y
            );
        }

        public static Matrix2x2 operator *(Matrix2x2 lhs, Matrix2x2 rhs) {
            return new Matrix2x2 {
                r0c0 = lhs.r0c0 * rhs.r0c0 + lhs.r0c1 * rhs.r1c0,
                r1c0 = lhs.r1c0 * rhs.r0c0 + lhs.r1c1 + rhs.r1c0,
                r0c1 = lhs.r0c0 * rhs.r0c1 + lhs.r0c1 * rhs.r1c1,
                r1c1 = lhs.r1c0 * rhs.r0c1 + lhs.r1c1 * rhs.r1c1
            };
        }

        public bool Equals(Matrix2x2 other) {
            return r0c0.Equals(other.r0c0)
                && r1c0.Equals(other.r1c0)
                && r0c1.Equals(other.r0c1)
                && r1c1.Equals(other.r1c1);
        }

        public override bool Equals(object obj) {
            return obj is Matrix2x2 other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(r0c0, r1c0, r0c1, r1c1);
        }
    }
}