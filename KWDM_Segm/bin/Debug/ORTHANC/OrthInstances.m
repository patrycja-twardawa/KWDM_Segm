function[instances] = OrthInstances(study, serie_id)
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    
    series_temp = OrthancGetSeries(URL, study); %% pobierz badania wybranego pacjenta (zapisywani w struct)
    
    for i = 1 : numel(series_temp{serie_id}.Instances)
        instances(i,:) = series_temp{serie_id}.Instances{i};
    end
end