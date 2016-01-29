using System;
using System.Collections.Generic;
using System.Windows;
using System.Runtime.InteropServices;

namespace ShoreNet
{
    /********************************************** PLEASE, DO NOT MODIFY THIS FILE! **********************************************/

    public class ShoreNetEngine : IDisposable
    {
        internal IntPtr ptr;


        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCCreateEngine([MarshalAs(UnmanagedType.LPStr)] String setupScript,
                                [MarshalAs(UnmanagedType.LPStr)] String setupCall);

        [DllImport("ShoreLib4.NET.dll")]
        public static extern IntPtr ShoreCDeleteEngine(IntPtr engine);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetDllDirectory(String lpPathName);

        public ShoreNetEngine(String setupScript, String setupCall)
        {
            ptr = ShoreCCreateEngine(setupScript, setupCall);
            if (ptr == IntPtr.Zero)
                throw new OutOfMemoryException();
        }

        public ShoreNetContent Process(IntPtr leftTop,
                        UInt32 width,
                        UInt32 height,
                        UInt32 planes,
                        Int32 pixelFeed,
                        Int32 lineFeed,
                        Int32 planeFeed,
                        String colorSpace)
        {
            return new ShoreNetContent(this, leftTop, width, height, planes, pixelFeed, lineFeed, planeFeed, colorSpace);
        }

        public void Dispose()
        {
            IntPtr tmp = ptr;

            if (tmp == IntPtr.Zero)
                return;

            ptr = IntPtr.Zero;
            ShoreCDeleteEngine(tmp);
            GC.SuppressFinalize(this);
        }
    }


    public class ShoreNetContent
    {
        internal IntPtr ptr;
        private ShoreNetEngine parent;

        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCEngineProcess(IntPtr engine,
                                 IntPtr leftTop,
                                 UInt32 width,
                                 UInt32 height,
                                 UInt32 planes,
                                 Int32 pixelFeed,
                                 Int32 lineFeed,
                                 Int32 planeFeed,
                                 [MarshalAs(UnmanagedType.LPStr)] String colorSpace);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern UInt32 ShoreCContentGetObjectCount(IntPtr content);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern UInt32 ShoreCContentGetInfoCount(IntPtr content);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCContentGetInfoKey(IntPtr content, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCContentGetInfo(IntPtr content, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCContentGetInfoOf(IntPtr content, [MarshalAs(UnmanagedType.LPStr)] String key);

        public ShoreNetContent(ShoreNetEngine engine,
                    IntPtr leftTop,
                    UInt32 width,
                    UInt32 height,
                    UInt32 planes,
                    Int32 pixelFeed,
                    Int32 lineFeed,
                    Int32 planeFeed,
                    String colorSpace)
        {
            parent = engine;
            ptr = ShoreCEngineProcess(engine.ptr, leftTop, width, height, planes, pixelFeed, lineFeed, planeFeed, colorSpace);
            if (ptr == IntPtr.Zero)
                throw new IndexOutOfRangeException();
        }

        public UInt32 GetObjectCount()
        {
            return ShoreCContentGetObjectCount(ptr);
        }

        public ShoreNetObject GetObject(UInt32 i)
        {
            return new ShoreNetObject(this, i);
        }

        public UInt32 GetInfoCount()
        {
            return ShoreCContentGetInfoCount(ptr);
        }

        public String GetInfoKey(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCContentGetInfoKey(ptr, i));
        }

        public String GetInfo(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCContentGetInfo(ptr, i));
        }

        public String GetInfoOf(String key)
        {
            return Marshal.PtrToStringAnsi(ShoreCContentGetInfoOf(ptr, key));
        }
    }


    public class ShoreNetRegion
    {
        private IntPtr ptr;
        private ShoreNetObject parent;

        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetRegion(IntPtr shObj);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCRegionGetBottom(IntPtr region);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCRegionGetLeft(IntPtr region);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCRegionGetRight(IntPtr region);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCRegionGetTop(IntPtr region);

        public ShoreNetRegion(ShoreNetObject shObj)
        {
            parent = shObj;
            ptr = ShoreCObjectGetRegion(shObj.ptr);
            if (ptr == IntPtr.Zero)
                throw new NullReferenceException();
        }

        public float GetBottom()
        {
            return ShoreCRegionGetBottom(ptr);
        }

        public float GetLeft()
        {
            return ShoreCRegionGetLeft(ptr);
        }

        public float GetRight()
        {
            return ShoreCRegionGetRight(ptr);
        }

        public float GetTop()
        {
            return ShoreCRegionGetTop(ptr);
        }
    }

    public class ShoreNetObject
    {
        internal IntPtr ptr;
        private ShoreNetContent parent;

        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCContentGetObject(IntPtr content, UInt32 i);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetType(IntPtr ptr);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern UInt32 ShoreCObjectGetMarkerCount(IntPtr ptr);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetMarkerKey(IntPtr ptr, UInt32 i);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern UInt32 ShoreCObjectGetAttributeCount(IntPtr ptr);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetAttributeKey(IntPtr ptr, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetAttribute(IntPtr ptr, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetAttributeOf(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] String key);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern UInt32 ShoreCObjectGetRatingCount(IntPtr ptr);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetRatingKey(IntPtr ptr, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCObjectGetRating(IntPtr ptr, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCObjectGetRatingOf(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] String key);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern UInt32 ShoreCObjectGetPartCount(IntPtr ptr);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetPartKey(IntPtr ptr, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetPart(IntPtr ptr, UInt32 i);
        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetPartOf(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] String key);

        private ShoreNetObject(ShoreNetContent parent, IntPtr ptr)
        {
            this.parent = parent;
            this.ptr = ptr;
        }

        public ShoreNetObject(ShoreNetContent shContent, UInt32 i)
            : this(shContent, ShoreCContentGetObject(shContent.ptr, i))
        {
            if (ptr == IntPtr.Zero)
                throw new NullReferenceException();
        }

        public ShoreNetObject(ShoreNetObject shObject, UInt32 i)
            : this(shObject.parent, ShoreCObjectGetPart(shObject.ptr, i))
        {
            if (ptr == IntPtr.Zero)
                throw new NullReferenceException();
        }

        public ShoreNetObject(ShoreNetObject shObject, String key)
            : this(shObject.parent, ShoreCObjectGetPartOf(shObject.ptr, key))
        {
            if (ptr == IntPtr.Zero)
                throw new NullReferenceException();
        }

        public String GetShoreType()
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetType(ptr));
        }

        public ShoreNetRegion GetRegion()
        {
            return new ShoreNetRegion(this);
        }


        public UInt32 GetMarkerCount()
        {
            return ShoreCObjectGetMarkerCount(ptr);
        }

        public String GetMarkerKey(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetMarkerKey(ptr, i));
        }

        public ShoreNetMarker GetMarker(UInt32 i)
        {
            return new ShoreNetMarker(this, i);
        }

        public ShoreNetMarker GetMarkerOf(String key)
        {
            return new ShoreNetMarker(this, key);
        }


        public UInt32 GetAttributeCount()
        {
            return ShoreCObjectGetAttributeCount(ptr);
        }

        public String GetAttributeKey(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetAttributeKey(ptr, i));
        }

        public String GetAttribute(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetAttribute(ptr, i));
        }

        public String GetAttributeOf(String key)
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetAttributeOf(ptr, key));
        }


        public UInt32 GetRatingCount()
        {
            
          return ShoreCObjectGetRatingCount(ptr);
        }

        public String GetRatingKey(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetRatingKey(ptr, i));
        }

        public float GetRating(UInt32 i)
        {
            return ShoreCObjectGetRating(ptr, i);
        }

        public float GetRatingOf(String key)
        {
            return ShoreCObjectGetRatingOf(ptr, key);
        }


        public UInt32 GetPartCount()
        {
            return ShoreCObjectGetPartCount(ptr);
        }

        public String GetPartKey(UInt32 i)
        {
            return Marshal.PtrToStringAnsi(ShoreCObjectGetPartKey(ptr, i));
        }

        public ShoreNetObject GetPart(UInt32 i)
        {
            return new ShoreNetObject(this, i);
        }

        public ShoreNetObject GetPartOf(String key)
        {
            return new ShoreNetObject(this, key);
        }
    }


    public class ShoreNetMarker
    {
        private IntPtr ptr;
        private ShoreNetObject parent;

        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetMarker(IntPtr obj, UInt32 i);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern IntPtr ShoreCObjectGetMarkerOf(IntPtr obj, [MarshalAs(UnmanagedType.LPStr)] String key);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCMarkerGetX(IntPtr marker);

        [DllImport("ShoreLib4.NET.dll")]
        private static extern float ShoreCMarkerGetY(IntPtr marker);

        private ShoreNetMarker(ShoreNetObject parent, IntPtr ptr)
        {
            this.parent = parent;
            this.ptr = ptr;
        }

        public ShoreNetMarker(ShoreNetObject obj, UInt32 i)
            : this(obj, ShoreCObjectGetMarker(obj.ptr, i))
        {
            if (ptr == IntPtr.Zero)
                throw new IndexOutOfRangeException();
        }

        public ShoreNetMarker(ShoreNetObject obj, String key)
            : this(obj, ShoreCObjectGetMarkerOf(obj.ptr, key))
        {
            if (ptr == IntPtr.Zero)
                throw new KeyNotFoundException();
        }

        public float GetX()
        {
            return ShoreCMarkerGetX(ptr);
        }

        public float GetY()
        {
            return ShoreCMarkerGetY(ptr);
        }
    }

}
