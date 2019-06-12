function [series] = OrthSeries(study)
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    
    series_temp = OrthancGetSeries(URL, study); %% pobierz badania wybranego pacjenta (zapisywani w struct)
    licznik = 1;
    
    for i = 1 : numel(series_temp)
        series(i,:) = series_temp{i}.ID;
        series_type{i} = series_temp{1,i}.MainDicomTags.BodyPartExamined;
        
        if(contains(series_type{i}, 'Segmentacja'))
            series_segm(licznik,:) = series_temp{i}.ID;
            licznik = licznik+1;
        end
    end
end