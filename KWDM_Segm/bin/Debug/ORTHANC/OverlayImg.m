function [out] = OverlayImg(path, orig, mask)
    orig = dicomread([path orig]);
    orig = rgb2gray(orig(:,:,:));
    dim = size(orig); %rozmiary obrazu

    win_size1 = floor((dim(1)/40)); %podzielnoœæ na 20
    win_size2 = floor((dim(2)/40));

    crop1 = win_size1*40; %ile pikseli mo¿na zostawiæ w obrazie, aby okna idealnie pasowa³y
    crop2 = win_size2*40;

    im_cropped = orig(1:crop1,1:crop2); %przycinanie obrazu
    
    mask = imread([path mask]);
    out = wljoin(double(im_cropped), double(mask), 0, 'ef');
    imwrite(out, fullfile(path, 'maskOver.png'), 'png');
end