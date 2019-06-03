function [series] = OrthancGetSeries(URL, studies)
    addpath('./jsonlab');
    series = loadjson(urlread([ URL '/studies/' studies '/series/' ]));
end
