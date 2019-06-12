function [] = OrthancSendDicom(file_path, dicomim, segmentation, DICOMfilename, DICOMfilename2, study)

    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
	
	% metadane takie same
    info = dicominfo(fullfile(file_path,dicomim));
    info.BodyPartExamined = 'Segmentacja';
    series = OrthSeries(study);
    number = [series(1:end) '.' num2str(series(end)+1)];
    info.SeriesInstanceUID = number;
    info.SOPInstaceUID = '1';
    
    % czytaj segmentacjê
    img = dicomread(fullfile(file_path, dicomim));  
    dicomwrite(img, fullfile(file_path,[DICOMfilename '.dcm']), info);

    % wyœlij na serwer
    execute = ['!curl -X POST ' URL '/instances --data-binary @' fullfile(file_path,[DICOMfilename '.dcm'])];
    eval(execute);
    
    % maska
    info2 = info;
    info2.SOPInstaceUID = '2';
    
    % czytaj segmentacjê
    img = imread(fullfile(file_path, segmentation));  
    dicomwrite(img, fullfile(file_path,[DICOMfilename2 '.dcm']), info2);

    % wyœlij na serwer
    execute = ['!curl -X POST ' URL '/instances --data-binary @' fullfile(file_path,[DICOMfilename2 '.dcm'])];
    eval(execute);
end


