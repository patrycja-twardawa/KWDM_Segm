function [series] = OrthSeries(study)
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    
    series_temp = OrthancGetSeries(URL, study); %% pobierz badania wybranego pacjenta (zapisywani w struct)
    
    for i = 1 : numel(series_temp)
        series(i,:) = series_temp{i}.ID;
    end
end