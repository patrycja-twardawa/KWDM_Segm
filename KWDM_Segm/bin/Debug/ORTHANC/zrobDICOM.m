function [] = zrobDICOM(dicom_inf, savepath, savename)
    inf = dicominfo(dicom_inf);
    image = imread(savepath);
    dicomwrite(image, [savepath(1:end-10) '\' savename '.dcm'], inf); 
end