
//

//
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenCVForUnity
{
    public class Tracking
    {

        //
        // C++:  Rect2d selectROI(Mat img, bool fromCenter = true)
        //

        //javadoc: selectROI(img, fromCenter)
        public static Rect2d selectROI (Mat img, bool fromCenter)
        {
            if (img != null)
                img.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            double[] tmpArray = new double[4];
            tracking_Tracking_selectROI_10 (img.nativeObj, fromCenter, tmpArray);
            Rect2d retVal = new Rect2d (tmpArray);
        
            return retVal;
            #else
            return null;
            #endif
        }

        //javadoc: selectROI(img)
        public static Rect2d selectROI (Mat img)
        {
            if (img != null)
                img.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            double[] tmpArray = new double[4];
            tracking_Tracking_selectROI_11 (img.nativeObj, tmpArray);
            Rect2d retVal = new Rect2d (tmpArray);
        
            return retVal;
            #else
            return null;
            #endif
        }


        //
        // C++:  Rect2d selectROI(String windowName, Mat img, bool showCrossair = true, bool fromCenter = true)
        //

        //javadoc: selectROI(windowName, img, showCrossair, fromCenter)
        public static Rect2d selectROI (string windowName, Mat img, bool showCrossair, bool fromCenter)
        {
            if (img != null)
                img.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            double[] tmpArray = new double[4];
            tracking_Tracking_selectROI_12 (windowName, img.nativeObj, showCrossair, fromCenter, tmpArray);
            Rect2d retVal = new Rect2d (tmpArray);
        
            return retVal;
            #else
            return null;
            #endif
        }

        //javadoc: selectROI(windowName, img)
        public static Rect2d selectROI (string windowName, Mat img)
        {
            if (img != null)
                img.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            double[] tmpArray = new double[4];
            tracking_Tracking_selectROI_13 (windowName, img.nativeObj, tmpArray);
            Rect2d retVal = new Rect2d (tmpArray);
        
            return retVal;
            #else
            return null;
            #endif
        }


        //
        // C++:  void selectROI(String windowName, Mat img, vector_Rect2d boundingBox, bool fromCenter = true)
        //

        //javadoc: selectROI(windowName, img, boundingBox, fromCenter)
        public static void selectROI (string windowName, Mat img, MatOfRect2d boundingBox, bool fromCenter)
        {
            if (img != null)
                img.ThrowIfDisposed ();
            if (boundingBox != null)
                boundingBox.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
            Mat boundingBox_mat = boundingBox;
            tracking_Tracking_selectROI_14 (windowName, img.nativeObj, boundingBox_mat.nativeObj, fromCenter);
        
            return;
            #else
            return;
            #endif
        }

        //javadoc: selectROI(windowName, img, boundingBox)
        public static void selectROI (string windowName, Mat img, MatOfRect2d boundingBox)
        {
            if (img != null)
                img.ThrowIfDisposed ();
            if (boundingBox != null)
                boundingBox.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
            Mat boundingBox_mat = boundingBox;
            tracking_Tracking_selectROI_15 (windowName, img.nativeObj, boundingBox_mat.nativeObj);
        
            return;
            #else
            return;
            #endif
        }


        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
        #else
        const string LIBNAME = "opencvforunity";
        #endif



        // C++:  Rect2d selectROI(Mat img, bool fromCenter = true)
        [DllImport(LIBNAME)]
        private static extern void tracking_Tracking_selectROI_10 (IntPtr img_nativeObj, bool fromCenter, double[] retVal);

        [DllImport(LIBNAME)]
        private static extern void tracking_Tracking_selectROI_11 (IntPtr img_nativeObj, double[] retVal);

        // C++:  Rect2d selectROI(String windowName, Mat img, bool showCrossair = true, bool fromCenter = true)
        [DllImport(LIBNAME)]
        private static extern void tracking_Tracking_selectROI_12 (string windowName, IntPtr img_nativeObj, bool showCrossair, bool fromCenter, double[] retVal);

        [DllImport(LIBNAME)]
        private static extern void tracking_Tracking_selectROI_13 (string windowName, IntPtr img_nativeObj, double[] retVal);

        // C++:  void selectROI(String windowName, Mat img, vector_Rect2d boundingBox, bool fromCenter = true)
        [DllImport(LIBNAME)]
        private static extern void tracking_Tracking_selectROI_14 (string windowName, IntPtr img_nativeObj, IntPtr boundingBox_mat_nativeObj, bool fromCenter);

        [DllImport(LIBNAME)]
        private static extern void tracking_Tracking_selectROI_15 (string windowName, IntPtr img_nativeObj, IntPtr boundingBox_mat_nativeObj);

    }
}
