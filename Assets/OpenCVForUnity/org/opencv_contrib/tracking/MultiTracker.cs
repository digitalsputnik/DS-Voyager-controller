
//

//
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenCVForUnity
{
// C++: class MultiTracker
//javadoc: MultiTracker
    public class MultiTracker : DisposableOpenCVObject
    {

        protected override void Dispose (bool disposing)
        {
#if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
            try {
                if (disposing) {
                }
                if (IsEnabledDispose) {
                    if (nativeObj != IntPtr.Zero)
                        tracking_MultiTracker_delete (nativeObj);
                    nativeObj = IntPtr.Zero;
                }
            } finally {
                base.Dispose (disposing);
            }
#else
            return;
#endif
        }

        protected internal MultiTracker (IntPtr addr) : base(addr)
        {
        }


        //
        // C++:   MultiTracker(String trackerType = "")
        //

        //javadoc: MultiTracker::MultiTracker(trackerType)
        public   MultiTracker (string trackerType)
        {
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            nativeObj = tracking_MultiTracker_MultiTracker_10 (trackerType);
        
            return;
            #else
            return null;
            #endif
        }

        //javadoc: MultiTracker::MultiTracker()
        public   MultiTracker ()
        {
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            nativeObj = tracking_MultiTracker_MultiTracker_11 ();
        
            return;
            #else
            return null;
            #endif
        }


        //
        // C++:  bool add(Mat image, Rect2d boundingBox)
        //

        //javadoc: MultiTracker::add(image, boundingBox)
        public  bool add (Mat image, Rect2d boundingBox)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            bool retVal = tracking_MultiTracker_add_10 (nativeObj, image.nativeObj, boundingBox.x, boundingBox.y, boundingBox.width, boundingBox.height);
        
            return retVal;
            #else
            return false;
            #endif
        }


        //
        // C++:  bool add(Mat image, vector_Rect2d boundingBox)
        //

        //javadoc: MultiTracker::add(image, boundingBox)
        public  bool add (Mat image, MatOfRect2d boundingBox)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();
            if (boundingBox != null)
                boundingBox.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
            Mat boundingBox_mat = boundingBox;
            bool retVal = tracking_MultiTracker_add_11 (nativeObj, image.nativeObj, boundingBox_mat.nativeObj);
        
            return retVal;
            #else
            return false;
            #endif
        }


        //
        // C++:  bool add(String trackerType, Mat image, Rect2d boundingBox)
        //

        //javadoc: MultiTracker::add(trackerType, image, boundingBox)
        public  bool add (string trackerType, Mat image, Rect2d boundingBox)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
        
            bool retVal = tracking_MultiTracker_add_12 (nativeObj, trackerType, image.nativeObj, boundingBox.x, boundingBox.y, boundingBox.width, boundingBox.height);
        
            return retVal;
            #else
            return false;
            #endif
        }


        //
        // C++:  bool add(String trackerType, Mat image, vector_Rect2d boundingBox)
        //

        //javadoc: MultiTracker::add(trackerType, image, boundingBox)
        public  bool add (string trackerType, Mat image, MatOfRect2d boundingBox)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();
            if (boundingBox != null)
                boundingBox.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
            Mat boundingBox_mat = boundingBox;
            bool retVal = tracking_MultiTracker_add_13 (nativeObj, trackerType, image.nativeObj, boundingBox_mat.nativeObj);
        
            return retVal;
            #else
            return false;
            #endif
        }


        //
        // C++:  bool update(Mat image, vector_Rect2d& boundingBox)
        //

        //javadoc: MultiTracker::update(image, boundingBox)
        public  bool update (Mat image, MatOfRect2d boundingBox)
        {
            ThrowIfDisposed ();
            if (image != null)
                image.ThrowIfDisposed ();
            if (boundingBox != null)
                boundingBox.ThrowIfDisposed ();
            #if UNITY_PRO_LICENSE || ((UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR) || UNITY_5 || UNITY_5_3_OR_NEWER
            Mat boundingBox_mat = boundingBox;
            bool retVal = tracking_MultiTracker_update_10 (nativeObj, image.nativeObj, boundingBox_mat.nativeObj);
        
            return retVal;
            #else
            return false;
            #endif
        }


        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
        #else
        const string LIBNAME = "opencvforunity";
        #endif



        // C++:   MultiTracker(String trackerType = "")
        [DllImport(LIBNAME)]
        private static extern IntPtr tracking_MultiTracker_MultiTracker_10 (string trackerType);

        [DllImport(LIBNAME)]
        private static extern IntPtr tracking_MultiTracker_MultiTracker_11 ();

        // C++:  bool add(Mat image, Rect2d boundingBox)
        [DllImport(LIBNAME)]
        private static extern bool tracking_MultiTracker_add_10 (IntPtr nativeObj, IntPtr image_nativeObj, double boundingBox_x, double boundingBox_y, double boundingBox_width, double boundingBox_height);

        // C++:  bool add(Mat image, vector_Rect2d boundingBox)
        [DllImport(LIBNAME)]
        private static extern bool tracking_MultiTracker_add_11 (IntPtr nativeObj, IntPtr image_nativeObj, IntPtr boundingBox_mat_nativeObj);

        // C++:  bool add(String trackerType, Mat image, Rect2d boundingBox)
        [DllImport(LIBNAME)]
        private static extern bool tracking_MultiTracker_add_12 (IntPtr nativeObj, string trackerType, IntPtr image_nativeObj, double boundingBox_x, double boundingBox_y, double boundingBox_width, double boundingBox_height);

        // C++:  bool add(String trackerType, Mat image, vector_Rect2d boundingBox)
        [DllImport(LIBNAME)]
        private static extern bool tracking_MultiTracker_add_13 (IntPtr nativeObj, string trackerType, IntPtr image_nativeObj, IntPtr boundingBox_mat_nativeObj);

        // C++:  bool update(Mat image, vector_Rect2d& boundingBox)
        [DllImport(LIBNAME)]
        private static extern bool tracking_MultiTracker_update_10 (IntPtr nativeObj, IntPtr image_nativeObj, IntPtr boundingBox_mat_nativeObj);

        // native support for java finalize()
        [DllImport(LIBNAME)]
        private static extern void tracking_MultiTracker_delete (IntPtr nativeObj);

    }
}
