// This is the main DLL file.

#define VISUALSALIENCY_EXPORTS

#include "VisualSaliency.h"

using namespace std;
using namespace cv;

CvCapture* capture;
String cascadeName, nestedCascadeName;

struct _VisualSaliency {
	_VisualSaliency(int width, int height, int type, int numtemporal, int numspatial, float firsttau, int firstrad) :
		salTracker(numtemporal, numspatial, firsttau, firstrad),
		width(width),
		height(height),
		type(type)
	{
		
	}
	FastSalience salTracker;
	Mat sal;
	Mat salRgb;
	int width;
	int height;
	int type;
	SalientPoint salientPoint;
};


VISUALSALIENCY_API int WINAPI InitCamera(CvCapture* capt)
{
	capture = capt;

	return 0;
}

VISUALSALIENCY_API int WINAPI ShowCamera()
{
	IplImage* iplImg = cvQueryFrame( capture );
	Mat frameCopy;
	Mat frame = iplImg;
	if( frame.empty() )
		return 0;
	if( iplImg->origin == IPL_ORIGIN_TL )
		frame.copyTo( frameCopy );
	else
		flip( frame, frameCopy, 0 );

	cv::imshow( "Rgb frame", frameCopy );

	return 0;
}

VISUALSALIENCY_API void WINAPI StopCamera()
{
	cvDestroyWindow("result");
}

VISUALSALIENCY_API VisualSaliency* WINAPI CreateVisualSaliency(int width, int height, int color, int numtemporal, int numspatial, float firsttau, int firstrad)
{
	return new VisualSaliency(width, height, color, numtemporal, numspatial, firsttau, firstrad);
}

VISUALSALIENCY_API void WINAPI DestroyVisualSaliency(VisualSaliency* vs)
{
	delete vs;
}

VISUALSALIENCY_API SalientPoint WINAPI GetSalientPoint(VisualSaliency* vs)
{
	return vs->salientPoint;
}

VISUALSALIENCY_API void* WINAPI GetSaliencyMap(VisualSaliency* vs)
{
	return vs->sal.ptr();
}

VISUALSALIENCY_API void* WINAPI GetSaliencyMapRgb(VisualSaliency* vs)
{
	normalize(vs->sal,vs->salRgb, 0, 256, NORM_MINMAX, CV_32F);
	vs->salRgb.convertTo(vs->salRgb,CV_8U);
	cvtColor(vs->salRgb,vs->salRgb,CV_GRAY2BGRA);
	return vs->salRgb.ptr();
}

VISUALSALIENCY_API int WINAPI GetSaliencyMapType(VisualSaliency* vs)
{
	return vs->sal.type();
}

VISUALSALIENCY_API void WINAPI UpdateVisualSaliency(VisualSaliency* vs, void* data)
{
	Mat dataFrame(vs->height, vs->width, vs->type, data);
	vs->salTracker.updateSalience(dataFrame);
	vs->salTracker.getSalImage(vs->sal);
	cout<< vs;

	double max;
	Point maxloc; 
	minMaxLoc(vs->sal, NULL, &max, NULL, &maxloc); 
	vs->salientPoint.x = maxloc.x;
	vs->salientPoint.y = maxloc.y;
	vs->salientPoint.frameWidth = vs->sal.cols;
	vs->salientPoint.frameHeight = vs->sal.rows;
}

/*
VISUALSALIENCY_API void WINAPI CalculateSaliencyMap(void* data, int row, int col, int color, int numtemporal, int numspatial, float firsttau, int firstrad)
{
	Mat sal;
	Mat dataFrame(row, col, color?CV_8UC3:CV_16UC1, data);
	FastSalience salTracker(numtemporal, numspatial, firsttau, firstrad); 
	salTracker.updateSalience(dataFrame);
	salTracker.getSalImage(sal);

	double min, max; 
	Point minloc, maxloc; 
	minMaxLoc(sal, NULL, &max, NULL, &maxloc); 
	sPoint2.x = maxloc.x;
	sPoint2.y = maxloc.y;
	sPoint2.frameWidth = sal.rows;
	sPoint2.frameHeight = sal.cols;

	circle(sal,maxloc,8, CV_RGB(0,0,255));
	circle(sal,maxloc,7, CV_RGB(0,255,255));
	circle(sal,maxloc,6, CV_RGB(255,0,255));
	circle(sal,maxloc,5, CV_GRAY2BGR );
	imshow("FastSUN Salience", sal);
}
*/

VISUALSALIENCY_API void WINAPI SaliencyMap( struct SalientPoint *sPoint, int numtemporal, int numspatial, float firsttau, int firstrad, int wFrameResized)
{
	FastSalience salTracker(numtemporal, numspatial, firsttau, firstrad);
	Mat im, im2, im2Copy, sal;

	if(capture)
	{
		IplImage* iplImg = cvQueryFrame( capture );
        im2 = iplImg;
        if( !im2.empty() )
		{
			if( iplImg->origin == IPL_ORIGIN_TL )
				im2.copyTo( im2Copy );
			else
				flip( im2, im2Copy, 0 );
		
			double ratio = wFrameResized * 1. / im2Copy.cols;
			if(!ratio > 0)
				cout << "Ratio < 0" << endl ;
			resize(im2Copy, im, Size(0,0), ratio, ratio, INTER_NEAREST); 
		
			salTracker.updateSalience(im);
			salTracker.getSalImage(sal);

			double min, max; 
			Point minloc, maxloc; 
			minMaxLoc(sal, NULL, &max, NULL, &maxloc); 
			sPoint->x = maxloc.x;
			sPoint->y = maxloc.y;
			sPoint->frameWidth = sal.rows;
			sPoint->frameHeight = sal.cols;

			circle(sal,maxloc,8, CV_RGB(0,0,255));
			circle(sal,maxloc,7, CV_RGB(0,255,255));
			circle(sal,maxloc,6, CV_RGB(255,0,255));
			circle(sal,maxloc,5, CV_GRAY2BGR );
			imshow("FastSUN Salience", sal);
		}
	}
}
