function [] = OrthancSendMask(file_path, segmentation, DICOMfilename, study, dicom)

    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
	
	% metadane takie same
    info = dicominfo(fullfile(file_path,dicom));
    info.BodyPartExamined = [info.BodyPartExamined ' Segmentacja'];
    series = OrthSeries(study);
    number = [series(1:end) '.' num2str(series(end)+1)];
    info.SeriesInstanceUID = number;
    info.SOPInstaceUID = '2';
    
    % czytaj segmentacjê
    img = imread(fullfile(file_path, segmentation));  
    dicomwrite(img, fullfile(file_path,[DICOMfilename '.dcm']), info);

    % wyœlij na serwer
    execute = ['!curl -X POST ' URL '/instances --data-binary @' fullfile(file_path,[DICOMfilename '.dcm'])];
    eval(execute);
end


